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
            var block = new BaseBlock(data.AsSpan(1));
            var calculatedHash = block.CalculateHash();
            if (calculatedHash == block.Hash)
            {
                // process the block
                // verify transactions...
                Console.WriteLine(" ");
            }
        }

        // for internal communication use the channel or weakreference
        // block generated or confirmed
        // process it
        Console.WriteLine("Processing Miner Event");
    }

}
