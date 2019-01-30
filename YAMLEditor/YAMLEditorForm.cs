using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using YamlDotNet.RepresentationModel;
using YamlDotNet.Core;
using System.Text.RegularExpressions;

namespace YAMLEditor
{
    public partial class YAMLEditorForm : Form
    {
        private Singleton nodeSingleton = Singleton.Instance();
        private CommandManager cManager = new CommandManager();
        private Subject subject = new Subject();
        private TreeObserver observer;

        private Director director = new Director();
        

        public YAMLEditorForm()
        {
            InitializeComponent();
            observer = new TreeObserver(mainTreeView);
            subject.Attach(observer);
            if (Program.hasRecover)
            {
                //popup
                DialogResult result = MessageBox.Show("Do you want to recover the last changes?", "Confirmation", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    onRecover(Program.path);
                }
                File.Delete(Program.path + "recover.yaml");
            }
        }

        private void onRecover(String path)
        {
            System.Diagnostics.Trace.WriteLine($"Filename: {"recover.yaml"}");
            Directory.SetCurrentDirectory(Path.GetDirectoryName(path + "recover.yaml") ?? "");

            mainTreeView.Nodes.Clear();
            var root = mainTreeView.Nodes.Add(Path.GetFileName(path +"recover.yaml"));
            root.ImageIndex = root.SelectedImageIndex = 3;
            LoadFile(root, "recover.yaml");
            root.Expand();
        }

        private void OnExit( object sender, EventArgs e )
        {
            Application.Exit();
        }

        private void OnOpen( object sender, EventArgs e )
        {
            var dialog = new OpenFileDialog()
                { Filter = @"Yaml files (*.yaml)|*.yaml|All files (*.*)|*.*", DefaultExt = "yaml" };
            if ( dialog.ShowDialog() == DialogResult.OK )
            {
                System.Diagnostics.Trace.WriteLine( $"Filename: {dialog.FileName}" );
                Directory.SetCurrentDirectory( Path.GetDirectoryName( dialog.FileName ) ?? "" );

                mainTreeView.Nodes.Clear();
                var root = mainTreeView.Nodes.Add( Path.GetFileName( dialog.FileName ) );
                root.ImageIndex = root.SelectedImageIndex = 3;
                LoadFile( root, dialog.FileName );
                root.Expand();
            }

            subject.Notify();
        }

        private void LoadFile( TreeNode node, string filename )
        {
            var yaml = new YamlStream();
            try
            {
                using ( var stream = new StreamReader( filename ) )
                {
                    yaml.Load( stream );
                }
            }
            catch ( Exception exception )
            {
                Console.WriteLine( exception.Message );
            }

            if ( yaml.Documents.Count == 0 ) return;
            LoadChildren( node, yaml.Documents [0].RootNode as YamlMappingNode );
        }

        private void LoadChildren( TreeNode root, YamlMappingNode mapping )
        {
            var children = mapping?.Children;
            if ( children == null ) return;

            foreach ( var child in children )
            {
                var key = child.Key as YamlScalarNode;
                System.Diagnostics.Trace.Assert( key != null );

                if ( child.Value is YamlScalarNode )
                {
                    var scalar = child.Value as YamlScalarNode;

                    var node = root.Nodes.Add( $"{key.Value}: {scalar.Value}" );
                    node.Tag = child;
                    node.ImageIndex = node.SelectedImageIndex = GetImageIndex( scalar );

                    if ( scalar.Tag == "!include" )
                    {
                        LoadFile( node, scalar.Value );
                    }
                }
                else if ( child.Value is YamlSequenceNode )
                {
                    var node = root.Nodes.Add( key.Value );
                    node.Tag = child.Value;
                    node.ImageIndex = node.SelectedImageIndex = GetImageIndex( child.Value );
                    LoadChildren( node, child.Value as YamlSequenceNode );
                }
                else if ( child.Value is YamlMappingNode )
                {
                    var node = root.Nodes.Add( key.Value );
                    node.Tag = child.Value;
                    node.ImageIndex = node.SelectedImageIndex = GetImageIndex( child.Value );

                    LoadChildren( node, child.Value as YamlMappingNode );
                }
            }
        }

        private int GetImageIndex( YamlNode node )
        {
            switch ( node.NodeType )
            {
                case YamlNodeType.Scalar:
                    if ( node.Tag == "!secret"  ) return 2;
                    if ( node.Tag == "!include" ) return 1;
                    return 0;
                case YamlNodeType.Sequence: return 3;
                case YamlNodeType.Mapping:
                    if ( node is YamlMappingNode mapping && mapping.Children.Any( pair => ( (YamlScalarNode) pair.Key ).Value == "platform" ) ) return 5;
                    return 4;
            }
            return 0;
        }

        private void LoadChildren( TreeNode root, YamlSequenceNode sequence )
        {
            foreach ( var child in sequence.Children )
            {
                if ( child is YamlSequenceNode )
                {
                    var node = root.Nodes.Add( root.Text );
                    node.Tag = child;
                    node.ImageIndex = node.SelectedImageIndex = GetImageIndex( child);

                    LoadChildren( node, child as YamlSequenceNode );
                }
                else if ( child is YamlMappingNode )
                {
                    var node = root.Nodes.Add( root.Text );
                    node.Tag = child;
                    node.ImageIndex = node.SelectedImageIndex = GetImageIndex( child );

                    LoadChildren( node, child as YamlMappingNode );
                }
                else if ( child is YamlScalarNode )
                {
                    var scalar = child as YamlScalarNode;
                    var node = root.Nodes.Add( scalar.Value );
                    node.Tag = child;
                    node.ImageIndex = node.SelectedImageIndex = GetImageIndex( child );
                }
            }
        }

        private void OnAfterSelect( object sender, TreeViewEventArgs e )
        {
            var imageIndex = mainTreeView.SelectedNode.ImageIndex;

            if (e.Node == mainTreeView.TopNode)
            {
                // label não e possivel alterar devido a fazer parte de multiple option
                OldValueTextB.Visible = false;
                newValueTextB.Visible = false;
                newTypeB.Visible = false;
                button1.Visible = false;
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                listBox1.Visible = false;
            }
            else
            {
                if (imageIndex == 0)
                {
                    newValueTextB.Visible = true;
                    OldValueTextB.Visible = true;
                    button1.Visible = true;
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    listBox1.Visible = true;
                    newTypeB.Visible = true;
                    label5.Visible = true;

                    int spaceIndex = e.Node.Text.IndexOf(" ") + 1;

                    var value = e.Node.Text.Substring(spaceIndex);

                    OldValueTextB.Text = value;
                    
                    var type = e.Node.Text.Substring(0, spaceIndex - 2);

                    newTypeB.Text = type;

                    if (e.Node.Parent.SelectedImageIndex == 3)   // multiple value
                    {
                        listBox1.SelectedIndex = 3;
                    }
                    else                                        // default
                    {
                        listBox1.SelectedIndex = 0;
                    }

                }
                else if (imageIndex == 2)                        //Password
                {
                    newValueTextB.Visible = true;
                    OldValueTextB.Visible = true;
                    button1.Visible = true;
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    listBox1.Visible = true;
                    newTypeB.Visible = false;
                    label5.Visible = false;

                    int spaceIndex = e.Node.Text.IndexOf(" ") + 1;

                    var value = e.Node.Text.Substring(spaceIndex);

                    OldValueTextB.Text = value;
                    
                    newTypeB.Text = "";

                    listBox1.SelectedIndex = 1;
                }
                else if (imageIndex == 3)                        //Multiple Option
                {
                    newValueTextB.Visible = true;
                    OldValueTextB.Visible = true;
                    button1.Visible = true;
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    listBox1.Visible = true;
                    newTypeB.Visible = false;
                    label5.Visible = false;

                    var value = e.Node.Text;

                    OldValueTextB.Text = value;
                    
                    newTypeB.Text = "";

                    listBox1.SelectedIndex = 2;
                }
                else if (imageIndex == 4)                        //Option
                {
                    if (e.Node.Parent.SelectedImageIndex == 3 && e.Node.Parent != mainTreeView.TopNode)
                    {
                        // label não e possivel alterar devido a fazer parte de multiple option

                        OldValueTextB.Visible = false;
                        newValueTextB.Visible = false;
                        newTypeB.Visible = false;
                        button1.Visible = false;
                        label1.Visible = false;
                        label2.Visible = false;
                        label3.Visible = false;
                        label4.Visible = false;
                        label5.Visible = false;
                        listBox1.Visible = false;


                    }
                    else
                    {
                        newValueTextB.Visible = true;
                        OldValueTextB.Visible = true;
                        button1.Visible = true;
                        label1.Visible = true;
                        label2.Visible = true;
                        label3.Visible = true;
                        label4.Visible = true;
                        listBox1.Visible = true;
                        newTypeB.Visible = false;
                        label5.Visible = false;

                        var value = e.Node.Text;

                        OldValueTextB.Text = value;
                        
                        newTypeB.Text = "";

                        listBox1.SelectedIndex = 4;
                    }

                }
                else if (imageIndex == 5)                        //Sensor
                {
                    newValueTextB.Visible = true;
                    OldValueTextB.Visible = true;
                    button1.Visible = true;
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    listBox1.Visible = true;
                    newTypeB.Visible = false;
                    label5.Visible = false;

                    var value = e.Node.Text;

                    OldValueTextB.Text = value;

                    newTypeB.Text = "";

                    listBox1.SelectedIndex = 4;
                }
            }
        }

        private void OnDoubleClick( object sender, EventArgs e )
        {
            if ( mainTreeView.SelectedNode == null ) return;
            var selected = mainTreeView.SelectedNode;

            if ( selected.Tag is YamlMappingNode node )
            {
                if ( node.Children.Any( p => ( (YamlScalarNode) p.Key ).Value == "platform" ) )
                {
                    var platform = node.Children.FirstOrDefault( p => ( (YamlScalarNode) p.Key ).Value == "platform" );
                    mainWebBrowser.Url = new Uri( $@"https://www.home-assistant.io/components/{ selected.Text }.{ platform.Value }" );
                    mainTabControl.SelectTab( helpTabPage );
                }
            }
        }
        
        private void onSave(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                var fileName = mainTreeView.TopNode.Text;
                var path = Environment.CurrentDirectory + @"\" + fileName;
                if (File.Exists(path))
                {
                    var fileText = nodeSingleton.convertTreeViewtoCode(mainTreeView);
                    using (Stream s = File.Open(fileName, FileMode.Create))
                    using (StreamWriter sw = new StreamWriter(s))
                    {
                        sw.Write(fileText);
                    }
                    File.Delete(Program.path + "recover.yaml");
                }
                else
                {
                    saveAsToolStripMenuItem_Click(sender, e);
                }
            }
        }
        
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                var fileText = nodeSingleton.convertTreeViewtoCode(mainTreeView);
                var dialog = new SaveFileDialog()
                    { Filter = @"Yaml files (*.yaml)|*.yaml|All files (*.*)|*.*", DefaultExt = "yaml", FileName = "HomeAssistantConf" };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (Stream s = File.Open(dialog.FileName, FileMode.Create))
                    using (StreamWriter sw = new StreamWriter(s))
                    {
                        sw.Write(fileText);
                    }
                    File.Delete(Program.path + "recover.yaml");
                }
            }
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            mainTreeView.Nodes.Clear();
            mainTreeView.Nodes.Add("new_file.yaml");
            mainTreeView.Nodes[0].ImageIndex = 3;
            mainTreeView.Nodes[0].SelectedImageIndex = 3;
        }
        
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                cManager.Undo();
                subject.Notify();
            }

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                cManager.Redo();
                subject.Notify();
            }

        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Press Help Tab!");
            
        }
        
        //Edit node
        private void EditNode(object sender, EventArgs e)
        {
            TreeNode node = mainTreeView.SelectedNode;
            cManager.addCommand(node, "edit");

            switch (listBox1.SelectedIndex)
            {
                case 0:
                    node.Text = newTypeB.Text + ": " + newValueTextB.Text;
                    OldValueTextB.Text = node.Text;
                    newTypeB.Text = "";
                    newValueTextB.Text = "";
                    break;

                case 1:
                    node.Text = "password: " + newValueTextB.Text;
                    OldValueTextB.Text = node.Text;
                    newTypeB.Text = "";
                    newValueTextB.Text = "";
                    break;

                case 2:
                    node.Text = newValueTextB.Text;
                    OldValueTextB.Text = node.Text;
                    newTypeB.Text = "";
                    newValueTextB.Text = "";
                    break;

                case 3:
                    node.Text = newTypeB.Text + ": " + newValueTextB.Text;
                    OldValueTextB.Text = node.Text;
                    newTypeB.Text = "";
                    newValueTextB.Text = "";
                    break;

                case 4:
                    node.Text = newValueTextB.Text;
                    OldValueTextB.Text = node.Text;
                    newTypeB.Text = "";
                    newValueTextB.Text = "";
                    break;
                    
                default:
                    break;
            }
            

            subject.Notify();

        }

        private void newNode_Click(object sender, EventArgs e)
        {
            
        }


        private void RemoveNode(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                TreeNode node = mainTreeView.SelectedNode;

                var aux = node;

                cManager.addCommand(aux, "remove");

                node.Remove();
                subject.Notify();
            }
            
        }

        private void newSameLvl(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                TreeNode node = mainTreeView.SelectedNode;
                AbstractBuilder singleNodeBuilder = new SingleNodeBuilder();

                director.Construct(singleNodeBuilder);

                var nodeToAdd = singleNodeBuilder.GetResult();

                node.Parent.Nodes.Add(nodeToAdd);

                int nChilds = node.Parent.GetNodeCount(false);

                cManager.addCommand(node.Parent.Nodes[nChilds - 1], "add");
                subject.Notify();
            }
           
        }

        private void newChild(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                TreeNode node = mainTreeView.SelectedNode;
                AbstractBuilder singleNodeBuilder = new SingleNodeBuilder();

                director.Construct(singleNodeBuilder);

                var nodeToAdd = singleNodeBuilder.GetResult();

                node.Nodes.Add(nodeToAdd);

                int nChilds = node.GetNodeCount(false);

                cManager.addCommand(node.Nodes[nChilds - 1], "add");
                subject.Notify();
            }
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TreeNode node = mainTreeView.SelectedNode;
            int spaceIndex;
            var value = "";
            switch (listBox1.SelectedIndex)
            {
                case 0:
                    newValueTextB.Visible = true;
                    OldValueTextB.Visible = true;
                    button1.Visible = true;
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    listBox1.Visible = true;
                    newTypeB.Visible = true;
                    label5.Visible = true;

                    spaceIndex = mainTreeView.SelectedNode.Text.IndexOf(" ") + 1;

                    value = mainTreeView.SelectedNode.Text.Substring(spaceIndex);
                    OldValueTextB.Text = value;
                    break;

                case 1:
                    newValueTextB.Visible = true;
                    OldValueTextB.Visible = true;
                    button1.Visible = true;
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    listBox1.Visible = true;
                    newTypeB.Visible = false;
                    label5.Visible = false;

                    spaceIndex = mainTreeView.SelectedNode.Text.IndexOf(" ") + 1;

                    value = mainTreeView.SelectedNode.Text.Substring(spaceIndex);
                    OldValueTextB.Text = value;
                    break;

                case 2:
                    newValueTextB.Visible = true;
                    OldValueTextB.Visible = true;
                    button1.Visible = true;
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    listBox1.Visible = true;
                    newTypeB.Visible = false;
                    label5.Visible = false;

                    spaceIndex = mainTreeView.SelectedNode.Text.IndexOf(" ") + 1;

                    value = mainTreeView.SelectedNode.Text.Substring(spaceIndex);
                    OldValueTextB.Text = value;
                    break;

                case 3:
                    newValueTextB.Visible = true;
                    OldValueTextB.Visible = true;
                    button1.Visible = true;
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    listBox1.Visible = true;
                    newTypeB.Visible = true;
                    label5.Visible = true;

                    spaceIndex = mainTreeView.SelectedNode.Text.IndexOf(" ") + 1;

                    value = mainTreeView.SelectedNode.Text.Substring(spaceIndex);
                    OldValueTextB.Text = value;
                    break;

                case 4:
                    newValueTextB.Visible = true;
                    OldValueTextB.Visible = true;
                    button1.Visible = true;
                    label1.Visible = true;
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    listBox1.Visible = true;
                    newTypeB.Visible = false;
                    label5.Visible = false;

                    spaceIndex = mainTreeView.SelectedNode.Text.IndexOf(" ") + 1;

                    value = mainTreeView.SelectedNode.Text.Substring(spaceIndex);
                    OldValueTextB.Text = value;
                    break;

                default:
                    break;
            }
        }

        private void multipleOptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                TreeNode node = mainTreeView.SelectedNode;
                AbstractBuilder multipleOptionBuilder = new MultipleOptionBuilder();

                director.Construct(multipleOptionBuilder);

                var nodeToAdd = multipleOptionBuilder.GetResult();

                node.Nodes.Add(nodeToAdd);

                int nChilds = node.GetNodeCount(false);

                cManager.addCommand(node.Nodes[nChilds - 1], "add");
                subject.Notify();
            }
        }

        private void conditionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                TreeNode node = mainTreeView.SelectedNode;
                AbstractBuilder conditionBuilder = new ConditionBuilder();

                director.Construct(conditionBuilder);

                var nodeToAdd = conditionBuilder.GetResult();

                node.Nodes.Add(nodeToAdd);

                int nChilds = node.GetNodeCount(false);

                cManager.addCommand(node.Nodes[nChilds - 1], "add");
                subject.Notify();
            }
        }

        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainTreeView.TopNode != null)
            {
                TreeNode node = mainTreeView.SelectedNode;
                AbstractBuilder optionBuilder = new OptionBuilder();

                director.Construct(optionBuilder);

                var nodeToAdd = optionBuilder.GetResult();

                node.Nodes.Add(nodeToAdd);

                int nChilds = node.GetNodeCount(false);

                cManager.addCommand(node.Nodes[nChilds - 1], "add");
                subject.Notify();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.home-assistant.io/cookbook/");
        }
    }
}