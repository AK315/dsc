using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommandLine;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using dsc.Model;
using dsc.Snmp;

using SnmpSharpNet;

var serviceProvider = new ServiceCollection()
    .AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddConsole();
        }
    )
    .AddTransient<IRouterDataSource, SnmpRouterDataSource>()
    .BuildServiceProvider();

var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<Program>();
//logger?.LogDebug("Start!");

// IConfiguration configuration = new ConfigurationBuilder()
//     .AddEnvironmentVariables()
//     .AddCommandLine(args)
//     .Build();
//IPAddress startAddress = configuration.GetValue<IPAddress>("address") ?? throw new ArgumentException("Parameter address is missing");

IPAddress? startAddress = null;

var executionParameters = Parser.Default.ParseArguments<ExecutionParameters>(args)
    .WithParsed<ExecutionParameters>(ep =>
    {
        string? strStartAddress = ep.StartAddress?.Trim();
        Regex regex = new Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}");
        
        if(!string.IsNullOrEmpty(strStartAddress) && regex.Matches(strStartAddress).Count() > 0)
            IPAddress.TryParse(ep.StartAddress?.Trim(), out startAddress);
    });

if(startAddress == null)
{
    Console.Write("Type IP address of the first router: ");
    var strStartIP = Console.ReadLine();

    if (string.IsNullOrEmpty(strStartIP))
    {
        Console.WriteLine("Error: IP address cannot be empty. ");
        Console.ReadLine();
        return;
    }

    Regex regex = new Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}");
    if(regex.Matches(strStartIP).Count() == 0 || !IPAddress.TryParse(strStartIP.Trim(), out startAddress))
    {
        Console.WriteLine("IP address of incorrect format");
        Console.ReadLine();
        return;
    }
}

Console.WriteLine($"Starting with IP address: {startAddress} ");

var rootRouterBuilder = new RouterBuilder(startAddress, serviceProvider.GetService<IRouterDataSource>());
var rootRouter = (Router) await rootRouterBuilder.Build();

//var l3Routers = new List<Router>();

var networkTopology = new Topology();

var builderTasks = new List<Task<Router>>();

if(rootRouter == null)
{
    Console.WriteLine("No router found");
    return;
}

networkTopology.AddNode(rootRouter);

if(rootRouter.NextHops != null)
    foreach(var nextHop in rootRouter.NextHops)
    {
        var builder = new RouterBuilder(nextHop, serviceProvider.GetService<IRouterDataSource>());
        builderTasks.Add(builder.Build());            
    }

Task.WaitAll(builderTasks.ToArray());

foreach(var task in builderTasks)
{
    var router = task.Result;
    if (router != null)
    {
        networkTopology.AddNode(router);
        networkTopology.AddLink(rootRouter, router);
    }
}

// SnmpWalker.v1GetNext();
// SnmpWalker.v2GetBulk();

// string ip = "172.0.0.1";
// //string oid = ".1.3.6.1.2.1.4.22";
// string oid = ".1.3.6.1.2.1.2.1.0";

// var snmpDeviceService = new SnmpDeviceService(IPAddress.Parse(ip));
// Console.WriteLine(await snmpDeviceService.GetIntValueAsync(oid));

// oid = ".1.3.6.1.2.1.2.2";

// var tableTask = snmpDeviceService.GetTableAsync(oid);
// var result = await tableTask;

//     if(result.Count <= 0)
//     {
//         Console.WriteLine("No results returned.\n");
//     } 
//     else 
//     {
//         // Console.Write("Instance");

//         // foreach( uint column in tableColumns ) 
//         // {
//         //     Console.Write("\tColumn id {0} |", column);
//         // }

//         Console.WriteLine("");

//         foreach( KeyValuePair<string, IDictionary<uint, AsnType>> kvp in result ) 
//         {
//             Console.Write("{0}", kvp.Key);

//             foreach(uint column in kvp.Value.Keys) 
//             {
//                 // if( kvp.Value.ContainsKey(column) ) 
//                 // {
//                     Console.Write("\t{0} ({1}) |", kvp.Value[column].ToString(), SnmpConstants.GetTypeName(kvp.Value[column].Type));
//                 // } 
//                 // else 
//                 // {
//                 //     Console.Write("\t|");
//                 // }
//             }
//             Console.WriteLine("");
//         }
//     }


Console.ReadLine();