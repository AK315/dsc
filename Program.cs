using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.InteropServices;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using CommandLine;
using dsc.Model;
using dsc.Snmp;
using dsc;

var executionParameters = Parser.Default.ParseArguments<ServiceOptions>(args)
    .MapResult<ServiceOptions, ExecutionParameters>(ExecutionParameters.FromServiceOptions, errors => null);

if(executionParameters == null || (executionParameters.StartAddress == null && executionParameters.IsService))
{
    Console.Write("Start IP address expexted");
    Environment.Exit(-1);
}

if(executionParameters.StartAddress == null)
    executionParameters.StartAddress = ReadIpAddress();

if(executionParameters.StartAddress == null)
{
    Console.Write("Start IP address expexted");
    Environment.Exit(-1);
}

var serviceProvider = new ServiceCollection()
    .AddLogging(builder =>
        {
            builder.SetMinimumLevel(executionParameters.LogLevel);
            builder.AddConsole();
        }
    )
    .AddTransient<IRouterDataSource, SnmpRouterDataSource>()
    .BuildServiceProvider();

var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<Program>();

if(executionParameters.IsService)
{

}
else
{
    await ExecuteConsole(executionParameters.StartAddress, serviceProvider, logger);

    Console.WriteLine("\r\nPress Enter to quit...");
    Console.ReadLine();
}

async Task ExecuteConsole(IPAddress startAddress, IServiceProvider serviceProvider, ILogger? logger)
{
    if(serviceProvider == null)
        throw new ArgumentNullException(nameof(serviceProvider));
    
    Console.WriteLine($"Starting with IP address: {startAddress} \r\n");

    var routerDataSource = serviceProvider.GetService<IRouterDataSource>();

    if (routerDataSource != null)
    {
        var topology = new Topology(routerDataSource, logger);
        await topology.BuildAsync(startAddress);

        Console.WriteLine(topology.ToString());
    }
}

IHostBuilder CreateCommonBuilder<TStartup>(string[] args, bool requireAppSettings, bool requireHostingJson, string serviceName, bool addUserSecrets = false) where TStartup : class
{
    if (args == null) 
        throw new ArgumentNullException(nameof(args));

    var builder = new HostBuilder();

    var executingAssembly = Assembly.GetExecutingAssembly().Location;
    var executingAssemblyDirectory = Path.GetFullPath(Path.GetDirectoryName(executingAssembly));

    builder.UseContentRoot(executingAssemblyDirectory);
    builder.ConfigureHostConfiguration(config =>
    {
        config.AddEnvironmentVariables(prefix: "DOTNET_");
        config.AddCommandLine(args);
    });

    builder.ConfigureAppConfiguration((hostingContext, config) =>
    {
        config
            .SetBasePath(executingAssemblyDirectory)
            .AddJsonFile("appsettings.json", optional: !requireAppSettings, reloadOnChange: true)
            .AddJsonFile("hosting.json", optional: !requireHostingJson, reloadOnChange: false);

        if (addUserSecrets)
        {
            config.AddUserSecrets<TStartup>();
        }

        config.AddEnvironmentVariables()
            .AddCommandLine(args);

        hostingContext.HostingEnvironment.ApplicationName = serviceName;
    });

    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    builder.ConfigureLogging((hostingContext, logging) =>
        {
            // IMPORTANT: This needs to be added *before* configuration is loaded, this lets
            // the defaults be overridden by the configuration.
            if (isWindows)
            {
                // Default the EventLogLoggerProvider to warning or above
                logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);
            }

            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            logging.AddConsole();
            logging.AddDebug();
            logging.AddEventSourceLogger();

            if (isWindows)
            {
                // Add the EventLogLoggerProvider on windows machines
                logging.AddEventLog();
            }
        })
        .UseDefaultServiceProvider((context, options) =>
        {
            var isDevelopment = context.HostingEnvironment.IsDevelopment();
            options.ValidateScopes = isDevelopment;
            options.ValidateOnBuild = isDevelopment;
        });

    builder.ConfigureWebHostDefaults(b => { b.UseStartup<TStartup>(); });
    builder.UseWindowsService();
    builder.UseSystemd();

    return builder;
}

IPAddress? ReadIpAddress()
{
    Console.Write("Type IP address of the first router: ");
    var strStartIP = Console.ReadLine();

    if (string.IsNullOrEmpty(strStartIP))
    {
        Console.WriteLine("Error: IP address cannot be empty. ");
        return null;
    }

    Regex regex = new Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}");
    if(regex.Matches(strStartIP).Count() == 0 || !IPAddress.TryParse(strStartIP.Trim(), out IPAddress? result))
    {
        Console.WriteLine("IP address of incorrect format");
        return null;
    }

    return result;
}