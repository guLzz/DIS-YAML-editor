using System.Windows.Forms;

namespace YAMLEditor
{
    class SingleNodeBuilder : AbstractBuilder
    {
        private TreeNode _node = new TreeNode();

        public override void AddChilds()
        {
            //single node has no childs
        }

        public override void BuildNode()
        {
            _node.Text = "key: value";
        }

        public override TreeNode GetResult()
        {
            return _node;
        }
    }
}
