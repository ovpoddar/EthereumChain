using API.Processors.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal record ProcessorModel(HttpListener Listener, MinerSocketProcessor WebSocketListener, SQLiteConnection SQLiteConnection);
