using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace API.Helpers;
internal readonly struct ArrayPoolUsing<T> : IDisposable
{
    private readonly ArrayPool<T> _arrayPool;
    private readonly bool clearArray;
    private readonly T[] _values;

    public ArrayPoolUsing(int length, bool clearArray = false)
    {
        this._arrayPool = ArrayPool<T>.Shared;
        this.clearArray = clearArray;

        _values = _arrayPool.Rent(length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Dispose() =>
        _arrayPool.Return(_values, clearArray);

    public static implicit operator T[](ArrayPoolUsing<T> arrayPoolUsing) =>
        arrayPoolUsing._values;

    public static implicit operator Span<T>(ArrayPoolUsing<T> arrayPoolUsing) =>
        new Span<T>(arrayPoolUsing._values);

    public T this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }
}
