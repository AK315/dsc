using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommandLine;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using dsc.Model;
using dsc.Snmp;

using SnmpSharpNet;

//logger?.LogDebug("Start!");

// IConfiguration configuration = new ConfigurationBuilder()
//     .AddEnvironmentVariables()
//     .AddCommandLine(args)
//     .Build();
//IPAddress startAddress = configuration.GetValue<IPAddress>("address") ?? throw new ArgumentException("Parameter address is missing");

IPAddress? startAddress = null;
LogLevel logLevel = LogLevel.Error;

var executionParameters = Parser.Default.ParseArguments<ExecutionParameters>(args)
    .WithParsed<ExecutionParameters>(ep =>
    {
        // Parsing start IP address
        string? strStartAddress = ep.StartAddress?.Trim();
        Regex regex = new Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}");
        
        if(!string.IsNullOrEmpty(strStartAddress) && regex.Matches(strStartAddress).Count() > 0)
            IPAddress.TryParse(ep.StartAddress?.Trim(), out startAddress);

        // Parsing log level
        if(!string.IsNullOrEmpty(ep.LogLevel))
            switch(ep.LogLevel.ToLower().Trim())
            {
                case "info":
                    logLevel = LogLevel.Information;
                    break;
                case "error":
                    logLevel = LogLevel.Error;
                    break;
                case "debug":
                    logLevel = LogLevel.Debug;
                    break;
            }
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

var serviceProvider = new ServiceCollection()
    .AddLogging(builder =>
        {
            builder.SetMinimumLevel(logLevel);
            builder.AddConsole();
        }
    )
    .AddTransient<IRouterDataSource, SnmpRouterDataSource>()
    .BuildServiceProvider();

var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<Program>();


Console.WriteLine($"Starting with IP address: {startAddress} \r\n");

var routerDataSource = serviceProvider.GetService<IRouterDataSource>();

if(routerDataSource != null)
{
    var topology = new Topology(routerDataSource, logger);
    await topology.BuildAsync(startAddress);

    Console.WriteLine(topology.ToString());
}

Console.WriteLine("\r\nPress any key to quit...");
Console.ReadLine();

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
