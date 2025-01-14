using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal class TransactionAddedEventArgs : MinerEventArgs
{
    public Guid TransactionId { get; }
    public string Transaction { get; }

    public TransactionAddedEventArgs(Guid transactionId, string transaction)
    {
        Transaction = transaction;
        TransactionId = transactionId;
    }

    protected override void ParseFromPacket(Span<byte> data)
    {
        throw new NotImplementedException();
    }
}
