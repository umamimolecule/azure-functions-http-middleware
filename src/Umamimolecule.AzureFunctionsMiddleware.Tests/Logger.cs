using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class Logger : ILogger
    {
        public List<LogLevel> EnabledLogLevels { get; set; }

        public Logger()
        {
            this.EnabledLogLevels = new List<LogLevel>
            {
                LogLevel.Trace,
                LogLevel.Debug,
                LogLevel.Information,
                LogLevel.Warning,
                LogLevel.Error,
                LogLevel.Critical
            };
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new DummyDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.EnabledLogLevels.Contains(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // TODO: Maybe want to track the log values but for now do nothing
        }
    }
}
