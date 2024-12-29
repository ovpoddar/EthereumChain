using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal class MinerEvents
{
    public class Transaction
    {
        public event EventHandler Added;
        public event EventHandler Updated;
    }
    public class Block
    {
        public event EventHandler Generated;
        public event EventHandler Confirmed;
    }
}
