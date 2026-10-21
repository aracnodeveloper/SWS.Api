namespace App.Modules.Logs;

public interface IAppLogger
{
    void Log(string message, LogType type = LogType.INFO);
}
