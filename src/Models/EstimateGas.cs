﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace API.Models;
internal struct EstimateGas
{
    public string? From { get; set; }
    public string? Value { get; set; }
    public string? Gas { get; set; }
    public string? GasPrice { get; set; }
    public string? Data { get; set; }
    public string To { get; set; }
}