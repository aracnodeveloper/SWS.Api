using Microsoft.Extensions.Logging;

namespace App.Modules.Logs;

/// <summary>Logger de consola. En una fase posterior puede persistir a una tabla de logs.</summary>
public class AppLogger : IAppLogger
{
    private readonly ILogger<AppLogger> _logger;

    public AppLogger(ILogger<AppLogger> logger)
    {
        _logger = logger;
    }

    public void Log(string message, LogType type = LogType.INFO)
    {
        switch (type)
        {
            case LogType.ERROR:
                _logger.LogError(message);
                break;
            case LogType.WARNING:
                _logger.LogWarning(message);
                break;
            default:
                _logger.LogInformation(message);
                break;
        }
    }
}
