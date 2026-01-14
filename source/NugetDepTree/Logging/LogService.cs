using System;
using System.Collections.Concurrent;

namespace NugetDepTree.Logging;

public class LogService : ILogService
{
    private readonly ConcurrentQueue<string> _logMessages = new();

    public event EventHandler<string>? LogMessageReceived;

    public void AddLogMessage(string message)
    {
        _logMessages.Enqueue(message);
        LogMessageReceived?.Invoke(this, message);
    }
}
