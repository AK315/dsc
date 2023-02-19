using Microsoft.Extensions.Logging;

class SnmpGetArpTable
{
    public SnmpGetArpTable(ILogger logger)
    {
        logger.LogDebug(GetType().Name);
    }
}