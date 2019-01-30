using System.Collections.Generic;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class RemoveNode : Command
    {
        protected TreeNode _tnode;
        protected TreeNode _parentNode;
        protected int _index;
        protected Node nodefunc = new Node();

        public RemoveNode(TreeNode tnode)
        {
            this._tnode = tnode;
            NodeInfo nodeInfo = new NodeInfo(tnode);
            this._parentNode = tnode.Parent;
            this._index = tnode.Index;

        }

        public override void Execute()
        {
            nodefunc.Exclude(_tnode);
        }

        public override void UnExecute()
        {
            Undo();
        }

        private void Undo()
        {
            nodefunc.Include(_tnode, _parentNode, _index);
        }
    }
}