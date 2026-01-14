using System;

namespace NugetDepTree.Logging;

public interface ILogService
{
    event EventHandler<string>? LogMessageReceived;
    void AddLogMessage(string message);
}
