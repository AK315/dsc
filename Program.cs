using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommandLine;
using Microsoft.Extensions.Configuration;
using System.Net;
using dsc.Snmp;

using SnmpSharpNet;

var serviceProvider = new ServiceCollection()
    .AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddConsole();
        }
    )
    .BuildServiceProvider();

var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<Program>();
logger?.LogDebug("Start!");

// IConfiguration configuration = new ConfigurationBuilder()
//     .AddEnvironmentVariables()
//     .AddCommandLine(args)
//     .Build();
//IPAddress startAddress = configuration.GetValue<IPAddress>("address") ?? throw new ArgumentException("Parameter address is missing");

IPAddress? startAddress;

var executionParameters = Parser.Default.ParseArguments<ExecutionParameters>(args)
    .WithParsed<ExecutionParameters>(ep =>
    {
        IPAddress.TryParse(ep.StartAddress?.Trim().ToCharArray(), out startAddress);
        Console.WriteLine($"Start IP address is {ep.StartAddress}");
    });


SnmpWalker.v1GetNext();
SnmpWalker.v2GetBulk();

string ip = "100.0.0.100";
//string oid = ".1.3.6.1.2.1.4.22";
string oid = ".1.3.6.1.2.1.2.2";

SnmpWalker.v2GetTable(ip, oid);

var snmpGetTable = new SnmpGetTable(IPAddress.Parse(ip), oid);
var result = await snmpGetTable.GetTableAsync();

    if(result.Count <= 0)
    {
        Console.WriteLine("No results returned.\n");
    } 
    else 
    {
        Console.Write("Instance");
        
        // foreach( uint column in tableColumns ) 
        // {
        //     Console.Write("\tColumn id {0} |", column);
        // }

        Console.WriteLine("");
        
        foreach( KeyValuePair<string, IDictionary<uint, AsnType>> kvp in result ) 
        {
            Console.Write("{0}", kvp.Key);
        
            foreach(uint column in kvp.Value.Keys) 
            {
                // if( kvp.Value.ContainsKey(column) ) 
                // {
                    Console.Write("\t{0} ({1}) |", kvp.Value[column].ToString(), SnmpConstants.GetTypeName(kvp.Value[column].Type));
                // } 
                // else 
                // {
                //     Console.Write("\t|");
                // }
            }
            Console.WriteLine("");
        }
    }


Console.ReadLine();