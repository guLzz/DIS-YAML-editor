using System.Windows.Forms;

namespace YAMLEditor
{
    class MultipleOptionBuilder : AbstractBuilder
    {
        private TreeNode _node = new TreeNode();

        public override void AddChilds()
        {
            _node.Nodes.Add("value");
            _node.Nodes.Add("value");

            for(int i = 0; i < _node.GetNodeCount(false); i++)
            {
                _node.Nodes[i].SelectedImageIndex = 4;
                _node.Nodes[i].ImageIndex = 4;
            }
        }

        public override void BuildNode()
        {
            _node.Text = "value";
            _node.SelectedImageIndex = 3;
            _node.ImageIndex = 3;
        }

        public override TreeNode GetResult()
        {
            return _node;
        }
    }
}