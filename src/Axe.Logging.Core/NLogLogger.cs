using NLog;

namespace Axe.Logging.Core
{
    public class NLogLogger : LoggerBase
    {
        readonly Logger logger;

        public NLogLogger(string  name) { logger = LogManager.GetLogger(name); }

        public override void RecordLogEntry(LogEntry logEntry)
        {
            var nlogLevel = GetNlogLevel(logEntry.Level);
            var logInfo = new
            {
                id = logEntry.AggregateId,
                time = logEntry.Time,
                data = logEntry.Data
            };

            logger.Log(nlogLevel, logInfo);
        }

        static LogLevel GetNlogLevel(AxeLogLevel logEntryLevel)
        {
            switch (logEntryLevel)
            {
                case AxeLogLevel.Info:
                    return LogLevel.Info;
                case AxeLogLevel.Warn:
                    return LogLevel.Warn;
                case AxeLogLevel.Fatal:
                    return LogLevel.Fatal;
                default:
                    return LogLevel.Error;
            }
        }
    }
}