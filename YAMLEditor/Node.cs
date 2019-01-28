using System.Windows.Forms;

namespace YAMLEditor
{
    public class Node
    {
        private int _curr = 0;

        public void Action(string nodevalue, int _nodeindex)
        {
            switch (nodevalue)
            {
                case "parent": _curr += _nodeindex; break;
                case "child": _curr -= _nodeindex; break;
            }
        }
    }
}