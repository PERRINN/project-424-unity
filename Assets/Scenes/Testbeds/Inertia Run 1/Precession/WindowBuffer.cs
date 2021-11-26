using UnityEngine;

public class WindowBuffer
{
    public readonly int windowSize;
    public readonly Vector3[] values;
    public int windowFillCount;

    private int count;

    public WindowBuffer(int windowSize)
    {
        this.windowSize = windowSize;
        values = new Vector3[windowSize];
    }

    public void Add(Vector3 newValue)
    {
        values[count++% windowSize] = newValue;
        windowFillCount = Mathf.Min(count, windowSize);
    }
}
