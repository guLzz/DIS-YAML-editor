using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class Node
    {
        public void Include(TreeView tview ,List<List<string>> info, TreeNode tnode)
        {
            var nOfMenber = info.Count;

            TreeNode root = tview.TopNode;

            var parent = searchParent(info[0], root);

            int index = Int32.Parse(info[0][2]);

            //parent[0].Nodes.Insert(0, tnode);
            parent.Nodes.Insert(index, tnode);
        }

        private TreeNode searchParent(List<string> nodeInfo, TreeNode node)
        {
            TreeNode parent = new TreeNode("empty");

            if (node.Text == nodeInfo[1] && node.Index == Int32.Parse(nodeInfo[3]))
            {
                parent = node;
            }
            else
            {
                int nChilds = node.GetNodeCount(false);

                if (nChilds> 0)
                {
                    for (int c = 0; c < nChilds; c++)
                    {
                        parent = searchParent(nodeInfo, node.Nodes[c]);
                        
                    }
                }
            }

            return parent;
        }

        public void Exclude(TreeNode tnode, List<List<string>> info)
        {
            tnode.Remove();
        }

        public void Edit(TreeNode tnode, List<List<string>> info)
        {
            tnode.Text = info[0][0];
        }
    }
}