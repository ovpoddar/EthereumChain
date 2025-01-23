// See https://aka.ms/new-console-template for more information
// check if chain is setup or not
// Pull the block chain from API some how or from mem pool or possibly from other nodes
//
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// todo want to use share memory to communicate with the API project
await Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((hostContext, services) => {
        services.AddHostedService<MinerWorker>();
        services.AddHostedService<SyncChainWorker>();
    })
    .Build().RunAsync();