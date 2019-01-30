using System.Windows.Forms;

namespace YAMLEditor
{
    class ConditionBuilder : AbstractBuilder
    {
        private TreeNode _node = new TreeNode();

        public override void AddChilds()
        {
            _node.Nodes.Add("condition");
            _node.Nodes.Add("condition");

            for (int i = 0; i < _node.GetNodeCount(false); i++)
            {
                _node.Nodes[i].SelectedImageIndex = 4;
                _node.Nodes[i].ImageIndex = 4;
            }

            for(int i = 0; i < _node.GetNodeCount(false); i++)
            {
                _node.Nodes[i].Nodes.Add("condition: state");
                _node.Nodes[i].Nodes.Add("entity_id: entity");
                _node.Nodes[i].Nodes.Add("state: state");
            }
        }

        public override void BuildNode()
        {
            _node.Text = "condition";
            _node.SelectedImageIndex = 3;
            _node.ImageIndex = 3;
        }

        public override TreeNode GetResult()
        {
            return _node;
        }
    }
}