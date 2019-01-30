using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class AddNode : Command
    {
        protected TreeNode _tnode;
        protected TreeNode _parentNode;
        protected int _index;
        protected Node nodefunc = new Node();

        public AddNode(TreeNode tnode)
        {
            this._tnode = tnode;
            this._parentNode = tnode.Parent;
            this._index = tnode.Index;

        }

        public override void Execute()
        {
            nodefunc.Include(_tnode, _parentNode, _index);
        }

        public override void UnExecute()
        {
            Undo();
        }

        private void Undo()
        {
            nodefunc.Exclude(_tnode);
        }
    }
}