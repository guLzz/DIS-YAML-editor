using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class Node
    {
        public void Include(TreeNode node, TreeNode parentNode, int index)
        {
            parentNode.Nodes.Insert(index,node) ;
        }
        
        public void Exclude(TreeNode tnode)
        {
            tnode.Remove();
        }

        public void Edit(TreeNode tnode, string text)
        {
            tnode.Text = text;
        }
    }
}