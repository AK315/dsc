using System.Net;
using CommandLine;

public class ExecutionParameters
{
    [Option('a', "address", Required = true, HelpText = "Defines an IP address of the first router device from where to start descovery.")]
//    public IPAddress? StartAddress {get; set; }
    public string? StartAddress {get; set; }
}