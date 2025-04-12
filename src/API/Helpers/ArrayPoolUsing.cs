using System.Buffers;
using System.Runtime.CompilerServices;

namespace API.Helpers;
internal struct ArrayPoolUsing<T> : IDisposable
{
    private readonly ArrayPool<T> _arrayPool;
    private readonly int _length;
    private readonly bool _clearArray;
    private T[]? _values;

    public ArrayPoolUsing(int length = 0, bool clearArray = false)
    {
        this._arrayPool = ArrayPool<T>.Shared;
        this._length = length;
        this._clearArray = clearArray;

        this.Rent(length);
    }

    public ArrayPoolUsing<T> Rent(int size)
    {
        if (size == 0) return this;
        ArgumentOutOfRangeException.ThrowIfNegative(size);

        if (_values != null)
            _arrayPool.Return(_values, _clearArray);
        _values = _arrayPool.Rent(size);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Dispose()
    {
        if (_values != null)
            _arrayPool.Return(_values, _clearArray);
    }

    public static implicit operator T[](ArrayPoolUsing<T> arrayPoolUsing) =>
        arrayPoolUsing._values ?? [];

    public static implicit operator Span<T>(ArrayPoolUsing<T> arrayPoolUsing) =>
        new Span<T>(arrayPoolUsing._values, 0, arrayPoolUsing._length);

    public readonly Span<T> Slice(int length)
    {
        if (length < 0 || length >= _length)
            throw new ArgumentOutOfRangeException(nameof(length));
        return new Span<T>(_values, 0, length);
    }

    public readonly T this[int index]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_values);
            return _values[index];
        }

        set
        {
            ArgumentNullException.ThrowIfNull(_values);
            _values[index] = value;
        }
    }

    public readonly Span<T> this[Range range]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_values);
            return _values[range];
        }
    }
}
