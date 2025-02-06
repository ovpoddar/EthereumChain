using API.Exceptions;
using API.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace API.Processors.Communication;
/// <summary>
/// <remarks>this is not thread safe.</remarks>
/// </summary>
internal class DataReceivedMemoryProcessor : IDisposable, ICommunication
{
    public object DisposeLock = new object();
    public bool IsDisposed { get; private set; }

    private readonly MemoryMappedFile _mmFile;
    private readonly MemoryMappedViewAccessor _accessor;
    private unsafe IntPtr _pointer;
    private readonly int _writeableSize;
    private readonly Action<byte[]> _action;
    private readonly ushort _processId;

    public unsafe DataReceivedMemoryProcessor(string name, bool isNameCreator, Action<byte[]> action)
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
        _action = action;
        new Thread(StartWorker).Start();
    }

    public MemoryMappedViewAccessor DataReceivedMemoryAccessors
    {
        get { EnsureNotDisposed(); return _accessor; }
    }

    public unsafe IntPtr Pointer
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

    unsafe public void Dispose()
    {
        lock (DisposeLock)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                _accessor.Dispose();
                _mmFile.Dispose();
                _pointer = IntPtr.Zero;
            }
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

    public unsafe void StartWorker()
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

}
