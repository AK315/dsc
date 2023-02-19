using Microsoft.Extensions.Logging;

class SnmpGetRouteTable
{
    public SnmpGetRouteTable(ILogger logger)
    {
        logger.LogDebug(GetType().Name);
    }
}