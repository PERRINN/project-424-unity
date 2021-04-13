namespace Perrinn424.Utilities
{
    public interface IIterator<out T>
    {
        T Current { get; }
        T MoveNext();
        T MovePrevious();
        void Reset();
    } 
}
