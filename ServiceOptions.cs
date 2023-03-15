using CommandLine;

namespace dsc;

//[Verb("--service", HelpText = "Starts application as a Windows service")]
public class ServiceOptions
{
    [Option('a', "address", Required = false, HelpText = "Defines an IP address of the first router device from where to start descovery.")]
    public string? StartAddress { get; set; }

    [Option('l', "loglevel", Required = false, Default = "error", HelpText = "Sets log level: info error debug")]
    public string? LogLevel { get; set; }

    [Option('s', "service", Required = false, Default = false, HelpText = "Starts in service/daemon mode")]
    public bool Service { get; set; } = false;
}
