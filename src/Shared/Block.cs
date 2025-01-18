
namespace Shared;

public class Block
{
    public int Number { get; set; }
    public string Hash { get; set; }
    public string ParentHash { get; set; }
    public string Nonce { get; set; }
    public string Sha3Uncles { get; set; }
    public string LogsBloom { get; set; }
    public string TransactionsRoot { get; set; }
    public string StateRoot { get; set; }
    public string ReceiptsRoot { get; set; }
    public string Miner { get; }
    public string Difficulty { get; set; }
    public string TotalDifficulty { get; set; }
    public string ExtraData { get; set; }
    public string Size { get; set; }
    public string GasLimit { get; set; }
    public string GasUsed { get; set; }
    public string TimeStamp { get; }
    public List<Transaction> Transactions { get; }
    public string[] Uncles { get; set; }

    public void AddTransaction(Transaction transaction) => 
        Transactions.Add(transaction);

    public Block(string minerAddress)
    {
        Miner = minerAddress;
        TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Transactions = [];
    }
}
