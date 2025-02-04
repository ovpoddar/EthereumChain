using API.Exceptions;
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
    public string Name { get; }

    private readonly MemoryMappedFile _mmFile;
    private readonly MemoryMappedViewAccessor _accessor;
    private unsafe IntPtr _pointer;
    private readonly int _writeableSize;
    private readonly Action<byte[]> _action;

    public unsafe DataReceivedMemoryProcessor(string name, bool isNameCreator, Action<byte[]> action)
    {
        _writeableSize = Setting.SharedMemorySize - sizeof(byte) - sizeof(int);
        Name = name;
        if (isNameCreator)
        {
            _mmFile = MemoryMappedFile.CreateNew(name, Setting.SharedMemorySize);
        }
        else
        {
#pragma warning disable CA1416 // Validate platform compatibility
            _mmFile = MemoryMappedFile.OpenExisting(name);
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

    // check the index access
    public unsafe void SendData(byte[] data)
    {
        var context = (byte*)_pointer.ToPointer();
        if (data.Length > _writeableSize)
        {
            var totalWritten = 0;
            while (totalWritten != data.Length)
            {
                if (context[0] == 0)
                {
                    Marshal.Copy(data,
                        totalWritten,
                        _pointer + sizeof(int) + sizeof(byte),
                        Math.Min(data.Length - totalWritten, _writeableSize));
                    context[0] = 1;
                }
                Task.Delay(100).Wait();
            }
            context[0] = 1;
        }
        else
        {
            while (true)
            {
                if (IsDisposed || context[0] == 0)
                {
                    Marshal.Copy(data,
                        0,
                        _pointer + sizeof(int) + sizeof(byte),
                        data.Length);
                    context[0] = 1;
                    break;
                }
            }
        }
    }

    public unsafe void StartWorker()
    {
        var context = (byte*)_pointer.ToPointer();
        Debug.Assert(_action != null);
        while (true)
        {
            if (IsDisposed)
                break;
            if (context[0] != 1)
                continue;


            Thread.MemoryBarrier();
            var data = new byte[Setting.SharedMemorySize];
            Marshal.Copy(_pointer + sizeof(int) + sizeof(byte),
                data, 
                0,
                Setting.SharedMemorySize);
            _action.Invoke(data);
            context[0] = 0;
        }
    }
}


/*******************************************************************************************
Sender class:
    has a method which takes the data.

after creating the class it will establish the communication channel

after calling the sender method it will do this

the full Shared buffer
***************************
^
set this value to 1, and print the message for remaining byte
state value can be like 
0. safe to write
1. safe to read

Receiver class:
    has a method which raised the event when the data is received.

after creating the class it will establish the communication channel

after binding the callback. it will run a continuous loop to check the state of the buffer.

***************************
^
checking this value if it is 1 then read the data and raise the event
and also have to set the value to 0 after reading the data
********************************************************************************************/