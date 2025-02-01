using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace API.Processors.Communication;
internal class DataReceivedMemoryProcessor : IDisposable, ICommunication
{
    public object DisposeLock = new object();
    public bool IsDisposed { get; private set; }
    public string Name { get; }

    readonly MemoryMappedFile _mmFile;
    readonly MemoryMappedViewAccessor _accessor;
    unsafe IntPtr _pointer;

    //TODO: So the MemoryMappedFile doesn't work on linux
    // and i don't want to implement linux staff yet that would be too 
    // much work for now so i will just leave it like this
    // and i will implement it later. https://github.com/rohitrajskk/shared_memory/blob/main/src/shm.h
    // this is a c repo that i will use to implement the shared memory on linux
    // might go follow this https://stackoverflow.com/questions/74840766/c-sharp-mono-linux-memory-mapped-files-shared-memory-multiple-processes
    public unsafe DataReceivedMemoryProcessor(string name, bool isNameCreator)
    {
        Name = name;
#pragma warning disable CA1416 // Validate platform compatibility
        _mmFile = MemoryMappedFile.OpenExisting(name);
#pragma warning restore CA1416 // Validate platform compatibility
        _accessor = _mmFile.CreateViewAccessor();
        _pointer = _accessor.SafeMemoryMappedViewHandle.DangerousGetHandle();
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
    public unsafe void SendDataAsync(byte[] data)
    {
        var context = (byte*)_pointer.ToPointer();
        if (data.Length > Setting.SharedMemorySize)
        {
                var totalWritten = 0;
                while (totalWritten != data.Length)
                {
                    if (context[0] == 0)
                    {
                        var writeSize = Math.Min(data.Length - totalWritten, Setting.SharedMemorySize);
                        Marshal.Copy(data[totalWritten..writeSize], totalWritten, _pointer + 1, writeSize);
                        context[0] = 1;
                    }
                    Task.Delay(100).Wait();
                }
                context[0] = 1;
        }
        else
        {
            Marshal.Copy(data, 0, _pointer + 1, data.Length);
            context[0] = 1;
        }
    }

    public Task<byte[]> ReceiveDataAsync()
    {
        throw new NotImplementedException();
    }
}


/************************************************************************************************************************************************************************************
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
************************************************************************************************************************************************************************************/