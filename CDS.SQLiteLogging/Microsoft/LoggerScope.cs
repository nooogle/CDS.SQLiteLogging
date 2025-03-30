using System;
using System.Collections.Generic;

namespace CDS.SQLiteLogging.Microsoft;

public class LoggerScope : IDisposable
{
    private readonly object state;
    private static readonly AsyncLocal<LoggerScope?> currentScope = new AsyncLocal<LoggerScope?>();

    public LoggerScope(object state)
    {
        this.state = state;
        Parent = currentScope.Value;
        currentScope.Value = this;
    }

    public LoggerScope? Parent { get; }

    public static LoggerScope? Current => currentScope.Value;

    public void Dispose()
    {
        if (currentScope.Value == this)
        {
            currentScope.Value = Parent;
        }
    }

    public override string ToString()
    {
        return state?.ToString() ?? string.Empty;
    }
}
