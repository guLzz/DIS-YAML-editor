using System.Windows.Forms;

namespace YAMLEditor
{
    public class TreeObserver : AbstractObserver
    {


        public TreeObserver()
        {
            // call new save class
        }

        public override void Update(AbstractSubject subject)
        {
            // execute save
            MessageBox.Show("Observer");
        }
    }
}