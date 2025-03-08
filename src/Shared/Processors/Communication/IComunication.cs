using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Processors.Communication;
public interface IApplicationCommunication
{
    void SendData(byte[] data);
    void ReceivedData(Action<byte[]> action);
}
