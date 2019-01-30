using System.Windows.Forms;

namespace YAMLEditor
{
    class OptionBuilder : AbstractBuilder
    {
        private TreeNode _node = new TreeNode();

        public override void AddChilds()
        {
            // has no childs
        }

        public override void BuildNode()
        {
            _node.Text = "value";
            _node.SelectedImageIndex = 4;
            _node.ImageIndex = 4;
        }

        public override TreeNode GetResult()
        {
            return _node;
        }
    }
}