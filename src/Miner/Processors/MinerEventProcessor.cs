using Shared.Core;
using Shared.Models;
using Shared.Processors.Communication;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miner.Processors;
internal static class MinerEventProcessor
{
    public static void ProcessEvent(ICommunication communication, SQLiteConnection connection, byte[] data)
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

                // verified tran
                response[1] = Convert.ToByte(true);
                communication.SendData(response);
                // store it to database
            }
        }

        // for internal communication use the channel or weakreference
        // block generated or confirmed
        // process it
        Console.WriteLine("Processing Miner Event");
    }

}
