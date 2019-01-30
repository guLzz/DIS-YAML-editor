using System.Windows.Forms;

namespace YAMLEditor
{
    public abstract class AbstractBuilder
    {
        public abstract void BuildNode();
        public abstract void AddChilds();
        public abstract TreeNode GetResult();
    }
}
