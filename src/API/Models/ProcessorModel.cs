using API.Processors.WebSocket;
using System.Data.SQLite;
using System.Net;

namespace API.Models;
internal record ProcessorModel(HttpListener Listener, MinerSocketProcessor WebSocketListener, SQLiteConnection SQLiteConnection);
