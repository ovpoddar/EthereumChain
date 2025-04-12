using Shared.Exceptions;
using Shared.Models;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Shared.Processors.Communication;

public class DataReceivedMemoryProcessor : IDisposable, IApplicationCommunication
{
    private readonly object DisposeLock = new();
    public bool IsDisposed { get; private set; }

    private readonly MemoryMappedFile _mmFile;
    private readonly MemoryMappedViewAccessor _accessor;
    private unsafe nint _pointer;
    private readonly int _writeableSize;
    private readonly ushort _processId;

    private Action<byte[]>? _action;

    public unsafe DataReceivedMemoryProcessor(string name, bool isNameCreator)
    {
        _writeableSize = Setting.SharedMemorySize - Marshal.SizeOf<SharedBufferContext>();
        if (isNameCreator)
        {
            _mmFile = MemoryMappedFile.CreateNew(name, Setting.SharedMemorySize);
            _processId = 0;
        }
        else
        {
#pragma warning disable CA1416 // Validate platform compatibility
            _mmFile = MemoryMappedFile.OpenExisting(name);
            _processId = 1;
#pragma warning restore CA1416 // Validate platform compatibility
        }
        _accessor = _mmFile.CreateViewAccessor();
        _pointer = _accessor.SafeMemoryMappedViewHandle.DangerousGetHandle();
    }

    private MemoryMappedViewAccessor DataReceivedMemoryAccessors
    {
        get { EnsureNotDisposed(); return _accessor; }
    }

    private unsafe nint Pointer
    {
        get { EnsureNotDisposed(); return _pointer; }
    }

    private void EnsureNotDisposed()
    {
        lock (DisposeLock)
        {
            if (!IsDisposed)
                return;
            throw new ObjectDisposedException(nameof(DataReceivedMemoryProcessor));
        }
    }

    public unsafe void Dispose()
    {
        lock (DisposeLock)
            if (!IsDisposed)
            {
                IsDisposed = true;
                _accessor.Dispose();
                _mmFile.Dispose();
                _pointer = nint.Zero;
            }
    }

    public unsafe void SendData(byte[] data)
    {
        var context = (SharedBufferContext*)_pointer.ToPointer();
        var totalWritten = 0;
        var isContextNotWritten = true;

        while (true)
        {
            if (IsDisposed || totalWritten == data.Length)
                break;
            if (context->State != 0)
                continue;
            if (isContextNotWritten)
            {
                context->IsContinues = (byte)(data.Length > _writeableSize ? 1 : 0);
                context->Size = data.Length;
                isContextNotWritten = false;
            }

            var writtenLength = Math.Min(data.Length - totalWritten, _writeableSize);
            Marshal.Copy(data,
                totalWritten,
                _pointer + Marshal.SizeOf<SharedBufferContext>(),
                writtenLength);
            totalWritten += writtenLength;
            context->ProcessId = _processId;
            context->State = 1;
        }
    }

    private unsafe void StartWorker()
    {
        var context = (SharedBufferContext*)_pointer.ToPointer();
        var read = 0;
        byte[] data = [];
        while (true)
        {
            if (IsDisposed)
                break;
            if (context->State != 1 || context->ProcessId != _processId)
                continue;

            Thread.MemoryBarrier();

            if (read == 0)
                data = new byte[context->Size];

            var readSize = Math.Min(context->Size - read, _writeableSize);

            Marshal.Copy(_pointer + Marshal.SizeOf<SharedBufferContext>(),
                   data,
                   read,
                   readSize);
            read += readSize;

            if (context->IsContinues == 0 || context->Size == read)
            {
                _action!.Invoke(data);
                read = 0;
                data = [];
            }
            context->State = 0;
        }
    }

    public void ReceivedData(Action<byte[]> action)
    {
        if (_action != null) throw new MultipleCallingException();
        _action = action;
        new Thread(StartWorker).Start();
    }
}
