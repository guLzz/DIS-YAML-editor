using System.Collections.Generic;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class RemoveNode : Command
    {
        protected TreeNode _tnode;
        protected TreeView _tview;
        protected List<List<string>> _info;
        protected Node nodefunc = new Node();

        public RemoveNode(TreeNode tnode, TreeView tview)
        {
            this._tnode = tnode;
            this._tview = tview;
            NodeInfo nodeInfo = new NodeInfo(tnode);
            this._info = nodeInfo.GetInfo();

        }

        public override void Execute()
        {
            nodefunc.Exclude(_tnode ,_info);
        }

        public override void UnExecute()
        {
            Undo(_info);
        }

        private void Undo(List<List<string>> info)
        {
            nodefunc.Include(_tview, _info, _tnode);
        }
    }
}