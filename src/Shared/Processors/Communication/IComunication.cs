namespace Shared.Processors.Communication;
public interface IApplicationCommunication
{
    void SendData(byte[] data);
    void ReceivedData(Action<byte[]> action);
}
