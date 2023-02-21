using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommandLine;
using Microsoft.Extensions.Configuration;
using System.Net;
using dsc.Snmp;

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
SnmpWalker.v2GetTable();

Console.ReadLine();