﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal enum MinerEventsTypes : byte
{
    TransactionAdded = 1,
    TransactionUpdated = 2,
    BlockGenerated = 3,
    BlockConfirmed = 4
}
