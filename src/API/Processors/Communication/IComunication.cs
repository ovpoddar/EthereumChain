using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Processors.Communication;
internal interface ICommunication
{
    Task SendDataAsync(byte[] data);
    Task<byte[]> ReceiveDataAsync();
}
