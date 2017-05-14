using System;

namespace Axe.Logging.Core
{
    public abstract class LoggerBase : IAxeLogger
    {
        public void Log(AxeLogLevel level, object data)
        {
            var logEntry = new LogEntry(Guid.NewGuid(), DateTime.UtcNow, data, level);
            RecordLogEntry(logEntry);
        }

        public abstract void RecordLogEntry(LogEntry logEntry);

        public void Log(Exception exception)
        {
            var logEntries = exception.GetLogEntry();
            foreach (var logEntry in logEntries)
            {
                RecordLogEntry(logEntry);
            }
        }

        public void Info(object data) { Log(AxeLogLevel.Info, data); }

        public void Error(object data) { Log(AxeLogLevel.Error, data); }

        public void Warn(object data) { Log(AxeLogLevel.Warn, data); }
    }
    
}