// See https://aka.ms/new-console-template for more information
// check if chain is setup or not
// Pull the block chain from API some how or from mem pool or possibly from other nodes
//
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Miner.Processors;
using Shared.Core;
using Shared.Models;
using Shared.Processors.Communication;
using Shared.Processors.Database;
using System.Data.SQLite;
using System.Threading.Channels;

var channel = Channel.CreateBounded<string>(1);
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
builder.Services.AddSingleton(StructureProcessor.InitializedDatabase());
builder.Services.AddSingleton(channel.Reader);
builder.Services.AddSingleton(channel.Writer);
builder.Services.AddSingleton<BlockChain>();

var app = builder.Build();

var communication = app.Services.GetRequiredService<ICommunication>();
var chain = app.Services.GetRequiredService<BlockChain>();
var writer = app.Services.GetRequiredService<ChannelWriter<string>>();
communication.ReceivedData(async (data) => await MinerEventProcessor.ProcessEvent(communication, chain, data, writer));
await app.RunAsync();