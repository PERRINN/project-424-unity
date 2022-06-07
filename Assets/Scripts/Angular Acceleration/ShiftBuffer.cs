public class ShiftBuffer<T>
{
    private T[] values;
    public readonly int length;
    public ShiftBuffer(int length)
    {
        this.length = length;
        values = new T[length];
    }

    public void Fill(T newValue)
    {
        for (int i = 0; i < length; i++)
        {
            values[i] = newValue;
        }
    }

    public void Push(T newValue)
    {
        for (int i = length - 1; i >= 1; i--)
        {
            values[i] = values[i - 1];
        }

        values[0] = newValue;
    }

    public T this[int index]
    {
        get => values[index];
    }
}
