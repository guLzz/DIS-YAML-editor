using System;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class NodeCommand : Command
    {
        private int _nodeindex;
        private TreeView _node;
        private string type;

        public NodeCommand(TreeView node, int nodeindex)
        {
            this._node = node;
            this._nodeindex = nodeindex;
        }

        public int NodeIndex
        {
            set { _nodeindex = value; }
        }

        public override void Execute()
        {
            //_node.Action(_nodevalue, type);
        }

        public override void UnExecute()
        {
            //_node.Action(Undo(_nodevalue), type);
        }

        private string Undo(string type)
        {
            switch (type)
            {
                case "remove": return "add";
                case "add": return "remove";
                case "edit": return "edit";
                default:
                    throw new ArgumentException("@nodevalue");
            }
        }
    }
}