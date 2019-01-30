using System.Collections.Generic;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class Subject : AbstractSubject
    {
        private List<AbstractObserver> obsList = new List<AbstractObserver>();

        public Subject()
        {
            
        }

        public override void Attach(AbstractObserver observer)
        {
            obsList.Add(observer);
        }

        public override void Dettach(AbstractObserver observer)
        {
            obsList.Remove(observer);
        }

        public override void Notify()
        {
            foreach(AbstractObserver obs in obsList)
            {
                obs.Update(this);
            }
        }
    }
}
