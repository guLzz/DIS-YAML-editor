using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class AddNode : Command
    {
        protected TreeNode _tnode;
        protected TreeView _tview;
        protected List<List<string>> _info;
        protected Node nodefunc = new Node();

        public AddNode(TreeNode tnode, TreeView tview)
        {
            this._tnode = tnode;
            this._tview = tview;
            NodeInfo nodeInfo = new NodeInfo(tnode);
            this._info = nodeInfo.GetInfo();

        }

        public override void Execute()
        {
            nodefunc.Include(_tview, _info, _tnode);
        }

        public override void UnExecute()
        {
            Undo(_info);
        }

        private void Undo(List<List<string>> info)
        {
            nodefunc.Exclude(_tnode, _info);
        }
    }
}