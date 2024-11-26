using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace src.Models;
internal struct EstimateGas
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("gas")]
    public string? Gas { get; set; }

    [JsonPropertyName("gasPrice")]
    public string? GasPrice { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("to")]
    [JsonRequired]
    public string To { get; set; }

}
