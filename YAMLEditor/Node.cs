using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class Node
    {
        private int _curr = 0;
        private string type;
        protected List<List<string>> nodeList = new List<List<string>>();

        public void Action(TreeNode node, string type)
        {
            if (node.GetNodeCount(false) > 0)
            {
                addNodeList(node);
            }
            switch (type)
            {
                case "remove":

                    break;
                case "add":

                    break;
                case "edit":

                    break;
                    
            }
        }

        private void addNodeList(TreeNode node)
        {
            nodeList.Add(new List<string> { node.Text, node.Parent.Text, node.Index.ToString() });
        }
    }
}