using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace dsc;

public class ExecutionParameters
{
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public bool IsService { get; set; } = false;

    public IPAddress? StartAddress { get; set; } = null;

    public LogLevel LogLevel { get; set; } = LogLevel.Error;


    /// <summary>
    /// Parses IP address
    /// </summary>
    /// <param name="strIpAddress">String representation of IP address</param>
    /// <returns></returns>
    private static IPAddress? ParesStartAddress(string? strIpAddress)
    {
        if (strIpAddress == null)
            return null;

        Regex regex = new Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}");

        IPAddress? startAddress = null;

        if (regex.Matches(strIpAddress.Trim()).Count() > 0)
            IPAddress.TryParse(strIpAddress?.Trim(), out startAddress);

        return startAddress;
    }


    /// <summary>
    /// Parses log level
    /// </summary>
    /// <param name="logLevel">String representation of log level</param>
    /// <returns></returns>
    private static LogLevel ParseLogLevel(string? logLevel)
    {
        LogLevel result = LogLevel.Error;

        if (!string.IsNullOrEmpty(logLevel))
            switch (logLevel.ToLower().Trim())
            {
                case "info":
                    result = LogLevel.Information;
                    break;
                case "error":
                    result = LogLevel.Error;
                    break;
                case "debug":
                    result = LogLevel.Debug;
                    break;
            }

        return result;
    }

    public static ExecutionParameters FromServiceOptions(ServiceOptions serviceOptions)
    {
        if (serviceOptions == null)
            throw new ArgumentNullException(nameof(serviceOptions));

        // if (!IsWindows)
        // {
        //     throw new InvalidOperationException("Failed to run agent as a service, because current platform is not Windows");
        // }

        return new ExecutionParameters
        {
            StartAddress = ParesStartAddress(serviceOptions.StartAddress),
            LogLevel = ParseLogLevel(serviceOptions.LogLevel),
//            IsService = serviceOptions.Service?.Trim().ToLower() == "false" ? false : true
            IsService = serviceOptions.Service
        };
    }
}