using Microsoft.Extensions.Logging;
using System;

namespace NugetDepTree.Logging;

public class UiLoggerProvider : ILoggerProvider
{
    private readonly ILogService _logService;

    public UiLoggerProvider(ILogService logService)
    {
        _logService = logService;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new UiLogger(categoryName, _logService);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
