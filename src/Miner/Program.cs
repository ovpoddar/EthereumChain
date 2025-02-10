// See https://aka.ms/new-console-template for more information
// check if chain is setup or not
// Pull the block chain from API some how or from mem pool or possibly from other nodes
//
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Core;
using Shared.Models;
using Shared.Processors.Communication;

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
{
    ApplicationName = "Miner",
    ContentRootPath = Directory.GetCurrentDirectory(),
    Args = args,
    DisableDefaults = true,
    Configuration = null
});

builder.Logging
    .ClearProviders()
    .AddConsole()
    .AddDebug();

builder.Services.AddHostedService<MinerWorker>();
builder.Services.AddSingleton<ICommunication>(new DataReceivedMemoryProcessor("EthereumChain", false));

var app = builder.Build();
var communication = app.Services.GetRequiredService<ICommunication>();
communication.ReceivedData((data) =>
{
    if (data[0] == (byte)CommunicationDataType.BaseBlock)
    {
        var block = new BaseBlock(data.AsSpan(1));
        var calculatedHash = block.CalculateHash();
        if (calculatedHash == block.Hash)
        {
            // process the block
            Console.WriteLine("Received data length: {0},\n type of result {1} ", data.Length, (CommunicationDataType)data[0]);
        }
    }

    // for internal communication use the channel or weakreference
    // block generated or confirmed
    // process it
});
await app.RunAsync();