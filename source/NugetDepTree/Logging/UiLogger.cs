using Microsoft.Extensions.Logging;
using System;

namespace NugetDepTree.Logging;

public class UiLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ILogService _logService;

    public UiLogger(string categoryName, ILogService logService)
    {
        _categoryName = categoryName;
        _logService = logService;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string logLevelString = logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRITICAL",
            _ => "UNKNOWN"
        };

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string message = formatter(state, exception);
        
        string formattedMessage = $"[{timestamp}] [{logLevelString}] [{_categoryName}] {message}";

        if (exception != null)
        {
            formattedMessage += Environment.NewLine + $"Exception: {exception}";
        }

        _logService.AddLogMessage(formattedMessage);
    }
}
