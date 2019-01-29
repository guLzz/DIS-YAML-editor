using System.Collections.Generic;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class EditNode : Command
    {
        protected TreeNode _tnode;
        protected List<List<string>> _info;
        protected Node nodefunc = new Node();

        public EditNode(TreeNode tnode)
        {
            this._tnode = tnode;
            NodeInfo nodeInfo = new NodeInfo(tnode);
            this._info = nodeInfo.GetInfo();

        }

        public override void Execute()
        {
            var aux = _tnode.Text;
            nodefunc.Edit(_tnode, _info);
            _info[0][0] = aux;
        }

        public override void UnExecute()
        {
            Undo(_info);
        }

        private void Undo(List<List<string>> _info)
        {
            var aux = _tnode.Text;
            nodefunc.Edit(_tnode, _info);
            _info[0][0] = aux;
        }
    }
}