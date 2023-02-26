using System.Net;
using CommandLine;

public class ExecutionParameters
{
    [Option('a', "address", Required = false, HelpText = "Defines an IP address of the first router device from where to start descovery.")]
    public string? StartAddress { get; set; }

    [Option('l', "loglevel", Required = false, Default = "error", HelpText = "Sets log level: info error debug")]
    public string? LogLevel { get; set; }
}