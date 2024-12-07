using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Models;
internal class Transaction
{
    public required string Hash { get; set; }
    public required string Nonce { get; set; }
    public required string BlockHash { get; set; }
    public required string BlockNUmber { get; set; }
    public required string TransactionIndex { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }
    public required string Value { get; set; }
    public required string Gas { get; set; }
    public required string GasPrice { get; set; }
    public required string Input { get; set; }
}
