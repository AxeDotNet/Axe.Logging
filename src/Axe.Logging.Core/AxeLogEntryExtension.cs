using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.Logging.Core
{
    public static class AxeLogEntryExtension
    {
        private const string LOG_ENTRY_KEY = "Axe_Logging";

        public static T Mark<T>(this T exception, LogEntryMark logEntry) where T : Exception
        {
            Validate(exception, logEntry);

            if (exception.Data[LOG_ENTRY_KEY] != null)
            {
                exception.Data.Remove(LOG_ENTRY_KEY);
            }

            exception.Data.Add(LOG_ENTRY_KEY, logEntry);
            return exception;
        }

        public static LogEntry[] GetLogEntry(this Exception exception, int maxLevel = 10)
        {
            if (exception == null)
            {
                return new List<LogEntry>().ToArray();
            }

            var logEntries = new List<LogEntry>();
            var aggreateId = Guid.NewGuid();
            if (FillLogEntries(exception, maxLevel, logEntries, aggreateId))
            {
                return logEntries.ToArray();
            }

            LogEntry defaultUknownException = CreateDefaultUknownException(aggreateId, exception);
            logEntries.Add(defaultUknownException);

            return logEntries.ToArray();
        }

        private static void Validate(Exception exception, LogEntryMark logEntry)
        {
            if (exception != null && logEntry != null) return;
            throw new ArgumentNullException(nameof(exception));
        }

        private static LogEntry CreateDefaultUknownException(Guid aggreateId, Exception exception)
        {
            return new LogEntry(aggreateId, DateTime.UtcNow, null, null, exception, LogLevel.Error);
        }

        private static bool FillLogEntries(Exception exception, int maxLevel, List<LogEntry> logEntries, Guid aggreateId, bool markedExceptionFound = false, int currentLevel = 1)
        {
            if (currentLevel > maxLevel) { return markedExceptionFound; }

            var logEntryObject = exception.Data[LOG_ENTRY_KEY];
            var logEntry = logEntryObject as LogEntryMark;
            if (logEntry != null)
            {
                var level = GetLogLevel(logEntry);
                var entry = new LogEntry(aggreateId, logEntry.Time, logEntry.Entry, logEntry.User, logEntry.Data, level);

                logEntries.Add(entry);
                markedExceptionFound = true;
            }
            
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                bool isGetLogEntryFromException = aggregateException.InnerExceptions.Any();

                foreach (Exception ex in aggregateException.InnerExceptions)
                {
                    if (ex == null) { continue; }
                    if (!FillLogEntries(ex, maxLevel, logEntries, aggreateId, currentLevel: currentLevel + 1))
                    {
                        isGetLogEntryFromException = false;
                    }
                }
                return isGetLogEntryFromException;
            }

            if (exception.InnerException == null)
            {
                return markedExceptionFound;
            }

            return FillLogEntries(exception.InnerException, maxLevel, logEntries, aggreateId, markedExceptionFound, currentLevel + 1);
        }

        private static LogLevel GetLogLevel(LogEntryMark logEntry)
        {
            switch (logEntry.Level)
            {
                case Level.DefinedByBusiness:
                    return LogLevel.Info;
                case Level.IKnowItWillHappen:
                    return LogLevel.Warning;
                case Level.Unknown:
                    return LogLevel.Error;
                default:
                    return LogLevel.Error;
            }
        }
    }
}