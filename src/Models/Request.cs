using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace src.Models;
// todo Update the processing concept over here.
internal struct Request
{
    [JsonPropertyName("method")]
    public string Method { get; set; }

    [JsonPropertyName("jsonrpc")]
    public string? RPCVersion { get; set; }

    [JsonPropertyName("params")]
    public string[]? Params { get; set; }

    [JsonPropertyName("id")]
    public object? Id { get; set; }
}
