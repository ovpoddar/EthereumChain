using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Processors.Communication;
internal class DataReceivedMemoryProcessor : IDisposable
{
    public object DisposeLock = new object();
    public bool IsDisposed { get; private set; }
    public string Name { get; }

    readonly MemoryMappedFile _mmFile;
    readonly MemoryMappedViewAccessor _accessor;
    unsafe byte* _pointer;

    //TODO: So the MemoryMappedFile doesn't work on linux
    // and i don't want to implement linux staff yet that would be too 
    // much work for now so i will just leave it like this
    // and i will implement it later. https://github.com/rohitrajskk/shared_memory/blob/main/src/shm.h
    // this is a c repo that i will use to implement the shared memory on linux
    // might go follow this tutorial https://www.youtube.com/watch?v=7R9o6wU2fZM
    public unsafe DataReceivedMemoryProcessor(string name)
    {
        Name = name;
#pragma warning disable CA1416 // Validate platform compatibility
        _mmFile = MemoryMappedFile.OpenExisting(name);
#pragma warning restore CA1416 // Validate platform compatibility
        _accessor = _mmFile.CreateViewAccessor();
        _pointer = (byte*)_accessor.SafeMemoryMappedViewHandle.DangerousGetHandle().ToPointer();
    }

    public MemoryMappedViewAccessor DataReceivedMemoryAccessors
    {
        get { EnsureNotDisposed(); return _accessor; }
    }

    public unsafe byte* Pointer
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
                _pointer = null;
            }
        }
    }
}
