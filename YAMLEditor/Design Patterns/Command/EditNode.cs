using System.Collections.Generic;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class EditNode : Command
    {
        protected TreeNode _tnode;
        protected string   _text;
        protected Node nodefunc = new Node();

        public EditNode(TreeNode tnode)
        {
            this._tnode = tnode;
            NodeInfo nodeInfo = new NodeInfo(tnode);
            this._text = tnode.Text;

        }

        public override void Execute()
        {
            var aux = _tnode.Text;
            nodefunc.Edit(_tnode, _text);
            _text = aux;
        }

        public override void UnExecute()
        {
            Undo();
        }

        private void Undo()
        {
            var aux = _tnode.Text;
            nodefunc.Edit(_tnode, _text);
            _text = aux;
        }
    }
}