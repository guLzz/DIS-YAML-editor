using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class NodeInfo
    {
        private TreeNode _tnode;
        protected List<List<string>> _info { get; set; }

        public NodeInfo(TreeNode tnode)
        {
            this._tnode = tnode;
            _info =  new List<List<string>>();
            GetList();

        }

        public List<List<string>> GetInfo()
        {
            return _info;
        }

        private void GetList()
        {
            var subNodeCount = _tnode.GetNodeCount(false);
            _info.Add(new List<string> { _tnode.Text, _tnode.Parent.Text, _tnode.Index.ToString(), _tnode.Parent.Index.ToString()}); //node e pai
            if (subNodeCount > 0)
            {
                AddNodeList(_tnode); //trata filhos do node
            }
        }

        private void AddNodeList(TreeNode node)
        {
            var nodeChild = node.GetNodeCount(false);
            for (int i = 0; i < nodeChild; i++)
            {
                _info.Add(new List<string> { node.Nodes[i].Text, node.Nodes[i].Parent.Text, node.Nodes[i].Index.ToString(), _tnode.Parent.Index.ToString() });
                if (node.Nodes[i].GetNodeCount(false) > 0)
                {
                    AddNodeList(node.Nodes[i]);
                }
            }
        }
    }
}