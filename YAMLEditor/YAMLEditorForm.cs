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
        public YAMLEditorForm()
        {
            InitializeComponent();
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
            mainPropertyGrid.SelectedObject = e.Node.Tag;

            int spaceIndex = e.Node.Text.IndexOf(" ") + 1;

            var value = e.Node.Text.Substring(spaceIndex);

            OldValueTextB.Text = value;

            if (spaceIndex > 0)
            {
                newTypeB.Visible = true;

                var type = e.Node.Text.Substring(0, spaceIndex - 2);

                newTypeB.Text = type;
            }
            else
            {
                newTypeB.Visible = false;
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

        private string convertTreeViewtoCode()
        {
            var code = "";

            var root = mainTreeView.TopNode;

            var rootNodeCount = root.GetNodeCount(false); //missing check if exists

            if(rootNodeCount > 0)
            {
               code = nodeToCodeRoot(root, rootNodeCount);
            }
            
            return code;
        }

        private string nodeToCodeRoot(TreeNode node, int subNodes)
        {
            var code = "";
            int tabsRequired = 0;               //tabs required for code building

            for (int i = 0; i < subNodes; i++)
            {
                code += nodeToCode(node.Nodes[i], tabsRequired, false);
            }

            return code;
        }

        private string nodeToCode(TreeNode node, int tabsRequired, bool hasHyphen)
        {
            var code = "";

            var tab = "  ";

            var subNodeCount = node.GetNodeCount(false);

            int spaceIndex = node.Text.IndexOf(" ");

            for (int t = 0; t < tabsRequired; t++)
            {
                code += tab;
            }

            if (spaceIndex == -1)
            {
                if(hasHyphen)
                {
                    code += "- " + node.Text + ":\n";
                }
                else
                {
                    code += node.Text + ":\n";
                }
            }
            else
            {
                if(hasHyphen)
                {
                    code += "- ";

                    var nodeKey = node.Text.Substring(0, spaceIndex - 1);
                    var nodeValue = node.Text.Substring(spaceIndex + 1);

                    bool containSymbol = false;
                    bool containsSpace = false;
                    bool containsSlash = false;

                    var regexItem = new Regex("^[a-zA-Z0-9 ].*$");

                    if (regexItem.IsMatch(nodeValue))
                    {
                        containSymbol = false;
                    }
                    else
                    {
                        containSymbol = true;
                    }


                    int aux = nodeValue.IndexOf(" ");

                    if (aux != -1)
                        containsSpace = true;

                    aux = nodeValue.IndexOf("/");

                    if (aux != -1)
                        containsSlash = true;

                    var firstChars = "";

                    if (nodeValue.Length > 1)
                    {
                        firstChars = nodeValue.Substring(0, 2);
                    }

                    if (firstChars == "{%")
                    {
                        code += nodeKey + ": >- \n" + nodeValue;

                    }
                    else if(nodeKey == "alias" || nodeKey == "at" || nodeKey == "api_key" || nodeKey == "target" || nodeKey == "message"
                            || nodeValue == "on" || nodeValue == "off" || nodeKey == "to" || nodeKey == "offset" || containSymbol 
                            || containsSpace || containsSlash)
                    {
                        code += nodeKey + ": '" + nodeValue + "'\n";
                    }
                    else if(nodeKey == "password")
                    {
                        code += nodeKey + ": !secret " + nodeValue + "\n";
                    }
                    else
                    {
                        code += node.Text + "\n";
                    }

                    
                }
                else
                {
                    var nodeKey = node.Text.Substring(0, spaceIndex - 1);
                    var nodeValue = node.Text.Substring(spaceIndex + 1);

                    bool containSymbol = false;
                    bool containsSpace = false;
                    bool containsSlash = false;

                    var regexItem = new Regex("^[a-zA-Z0-9 ].*$");
                    
                    if (regexItem.IsMatch(nodeValue))
                    {
                        containSymbol = false;
                    }
                    else
                    {
                        containSymbol = true;
                    }

                    int aux = nodeValue.IndexOf(" ");

                    if (aux != -1)
                        containsSpace = true;

                    aux = nodeValue.IndexOf("/");

                    if (aux != -1)
                        containsSlash = true;

                    var firstChars = "";

                    if (nodeValue.Length > 1)
                    {
                        firstChars = nodeValue.Substring(0, 2);
                    }

                    if (firstChars == "{%")
                    {
                        code += nodeKey + ": >- \n" + nodeValue;

                    }
                    else if (nodeKey == "alias" || nodeKey == "at" || nodeKey == "api_key" || nodeKey == "target" || nodeKey == "message"
                            || nodeValue == "on" || nodeValue == "off" || nodeKey == "to" || nodeKey == "offset" || containSymbol
                            || containsSpace || containsSlash)
                    {
                        code += nodeKey + ": '" + nodeValue + "'\n";
                    }
                    else if (nodeKey == "password")
                    {
                        code += nodeKey + ": !secret " + nodeValue + "\n";
                    }
                    else
                    {
                        code += node.Text + "\n";
                    }
                }
            }
            
            if(node.SelectedImageIndex == 3)
            {
                for(int i = 0; i < subNodeCount; i++)
                {
                    var subSubNodeCount = node.Nodes[i].GetNodeCount(false);
                    
                    for(int j = 0; j < subSubNodeCount; j++)
                    {
                        if(j == 0)
                        {
                            code += nodeToCode(node.Nodes[i].Nodes[j], tabsRequired, true);
                        }
                        else
                        {
                            code += nodeToCode(node.Nodes[i].Nodes[j], tabsRequired + 1, false);
                        }
                    }
                    if(tabsRequired ==0)
                        code += "\n";
                }
            }
            else
            {
                for (int i = 0; i < subNodeCount; i++)
                {
                    code += nodeToCode(node.Nodes[i], tabsRequired + 1, false);
                }
            }

            return code;
        }

        //secalhar usar isto para save as e redifenir um novo save para apenas substituir o ficheiro ja existente
        private void onSave(object sender, EventArgs e)
        {
            var fileText = convertTreeViewtoCode();
            var fileName = mainTreeView.TopNode.Text;
            var path = Environment.CurrentDirectory + @"\" + fileName;
            using (Stream s = File.Open(fileName, FileMode.Create ))
            using (StreamWriter sw = new StreamWriter(s))
            {
                sw.Write(fileText);
            }
        }


        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileText = convertTreeViewtoCode();
            var dialog = new SaveFileDialog()
                { Filter = @"Yaml files (*.yaml)|*.yaml|All files (*.*)|*.*", DefaultExt = "yaml", FileName = "HomeAssistantConf" };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream s = File.Open(dialog.FileName, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(s))
                {
                    sw.Write(fileText);
                }
            }
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            mainTreeView.Nodes.Clear();
            mainTreeView.Nodes.Add("type: value");
            doWizard();

        }

        private void doWizard()
        {
            
        }

        //autosave, falta parte em que chama a funcao cada vez que acontece algo novo
        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            var fileText = convertTreeViewtoCode();
            var path = Environment.CurrentDirectory + @"\recover.yaml";

            using (Stream s = File.Open(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(s))
                {
                    File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
                    sw.Write(fileText);
                }
            }
        }

        private void mainPropertyGrid_Click(object sender, EventArgs e)
        {
            MessageBox.Show("main click");
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("edit");

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("undo");

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("redo");

        }

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("cut");

        }

        private void copyToolStripButton_Click1(object sender, EventArgs e)
        {
            MessageBox.Show("copy");

        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("paste");

        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("halp");

        }

        private void mainPropertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
           
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode node = mainTreeView.SelectedNode;

            node.Text = newTypeB.Text + ": " + newValueTextB.Text;

            //update old textbox
            int spaceIndex = mainTreeView.SelectedNode.Text.IndexOf(" ") + 1;

            var value = mainTreeView.SelectedNode.Text.Substring(spaceIndex);

            OldValueTextB.Text = value;

            if (spaceIndex > 0)
            {
                var type = mainTreeView.SelectedNode.Text.Substring(0, spaceIndex - 2);

                newTypeB.Text = type;
            }
            else
                newTypeB.Text = "";
        }

        private void newNode_Click(object sender, EventArgs e)
        {
            mainTreeView.SelectedNode.Nodes.Add("type: value");
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        
    }
}