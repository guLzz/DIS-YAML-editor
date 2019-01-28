using System;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class NodeCommand : Command
    {
        private int _nodeindex;
        private TreeView _node;

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
            _node.Action(_nodevalue, _nodeindex);
        }

        public override void UnExecute()
        {
            _node.Action(Undo(_nodevalue), _nodeindex);
        }

        private string Undo(string nodevalue)
        {
            switch (@nodevalue)

            {
                case "child": return "parent";
                case "parent": return "child";
                default:
                    throw new ArgumentException("@nodevalue");
            }
        }
    }
}