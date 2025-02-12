using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace API.Helpers;
internal readonly struct ArrayPoolUsing<T> : IDisposable
{
    private readonly ArrayPool<T> _arrayPool;
    private readonly bool _clearArray;
    private readonly T[] _values;

    public ArrayPoolUsing(int length, bool clearArray = false)
    {
        this._arrayPool = ArrayPool<T>.Shared;
        this._clearArray = clearArray;

        _values = _arrayPool.Rent(length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Dispose() =>
        _arrayPool.Return(_values, _clearArray);

    public static implicit operator T[](ArrayPoolUsing<T> arrayPoolUsing) =>
        arrayPoolUsing._values;

    public static implicit operator Span<T>(ArrayPoolUsing<T> arrayPoolUsing) =>
        new Span<T>(arrayPoolUsing._values);

    public Span<T> Slice(int length)
    {
        if (length < 0 || length > _values.Length)
            throw new ArgumentOutOfRangeException(nameof(length));
        return new Span<T>(_values, 0, length);
    }

    public T this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }
}
