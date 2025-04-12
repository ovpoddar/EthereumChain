using Shared.Core;
using Shared.Models;
using Shared.Processors.Communication;
using System.Threading.Channels;

namespace Miner.Processors;
internal static class MinerEventProcessor
{
    public static async ValueTask ProcessEvent(IApplicationCommunication communication, BlockChain chain, byte[] data, ChannelWriter<string> writer)
    {
        if (data[0] == (byte)CommunicationDataType.BaseBlock)
        {

            byte[] response =
            [
                (byte)CommunicationDataType.StatusBlock,
                Convert.ToByte(false)
            ];

            var block = new BaseBlock(data.AsSpan(1));
            var calculatedHash = block.CalculateHash();
            if (calculatedHash == block.Hash)
            {
                foreach (var item in block.Transactions)
                {
                    if (!Transaction.IsValidTransaction(item.RawTransaction))
                    {
                        communication.SendData(response);
                        return;
                    }
                }
                var transactionStatus = await chain.AddBlock(block);
                response[1] = Convert.ToByte(transactionStatus);
                if (transactionStatus) await writer.WriteAsync(calculatedHash);
            }
            communication.SendData(response);
        }


        Console.WriteLine("Processing Miner Event");
    }

}
