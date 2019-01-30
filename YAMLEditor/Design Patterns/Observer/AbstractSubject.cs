namespace YAMLEditor
{
    public abstract class AbstractSubject
    {
        public abstract void Attach(AbstractObserver observer);
        public abstract void Dettach(AbstractObserver observer);
        public abstract void Notify();
    }
}
