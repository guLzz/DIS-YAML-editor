using System.Windows.Forms;

namespace YAMLEditor
{
    class Director
    {
        public void Construct(AbstractBuilder builder)
        {
            builder.BuildNode();
            builder.AddChilds();
        }
    }
}
