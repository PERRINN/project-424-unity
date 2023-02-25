using System;
using System.Collections.Generic;

//https://en.wikipedia.org/wiki/Row-_and_column-major_order
//https://eli.thegreenplace.net/2015/memory-layout-of-multi-dimensional-arrays
public class DenseArray<TCollection, TType> where TCollection:IList<TType>
{
    public TCollection collection;
    public int row;
    public int column;

    public DenseArray()
    {
    }

    public DenseArray(int row, int column)
    {
        this.row = row;
        this.column = column;

        collection = (TCollection)Activator.CreateInstance(typeof(TCollection), row*column);
    }

    public TType this[int index]
    {
        set => collection[index] = value;
        get => collection[index];
    }
}

[Serializable]
public class SerializedDenseArray : DenseArray<float[], float>
{
    public SerializedDenseArray()
    {
    }

    public SerializedDenseArray(int row, int column) : base(row, column){    }
}
