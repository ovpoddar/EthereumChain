using Nethereum.Merkle.Patricia;
using Shared.Core;
using Shared.Models;
using Shared.Processors.Communication;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Miner.Processors;
internal static class MinerEventProcessor
{
    public static async ValueTask ProcessEvent(ICommunication communication, BlockChain chain, byte[] data, ChannelWriter<string> writer)
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

                response[1] = Convert.ToByte(true);
                communication.SendData(response);
                var task1 = chain.AddBlock(block);
                var task2 = writer.WriteAsync(calculatedHash).AsTask();
                await Task.WhenAll(task1, task2);
            }
        }

        
        Console.WriteLine("Processing Miner Event");
    }

}
