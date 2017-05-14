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
            var logEntries = new List<LogEntry>();
            if (exception == null)
            {
                return logEntries.ToArray();
            }

            var aggreateId = Guid.NewGuid();

            FillLogEntries(exception, maxLevel, logEntries, aggreateId);

            if (!IsAllBranchesMarkedLogEntry(exception, maxLevel))
            {
                LogEntry defaultUknownException = CreateDefaultUknownException(aggreateId, exception);
                logEntries.Add(defaultUknownException);
            }

            return logEntries.ToArray();
        }

        private static void Validate(Exception exception, LogEntryMark logEntry)
        {
            if (exception != null && logEntry != null) return;
            throw new ArgumentNullException(nameof(exception));
        }

        private static LogEntry CreateDefaultUknownException(Guid aggreateId, Exception exception)
        {
            return new LogEntry(aggreateId, DateTime.UtcNow, exception, AxeLogLevel.Error);
        }

        private static void FillLogEntries(Exception exception, int maxLevel, List<LogEntry> logEntries, Guid aggreateId, int currentLevel = 1)
        {
            if(exception == null) return;
            if (currentLevel > maxLevel) return; 

            var logEntryObject = exception.Data[LOG_ENTRY_KEY];
            var logEntry = logEntryObject as LogEntryMark;
            if (logEntry != null)
            {
                AxeLogLevel level = GetLogLevel(logEntry);
                var entry = new LogEntry(aggreateId, logEntry.Time, logEntry.Data, level);

                logEntries.Add(entry);
            }

            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                FillLogEntriesForAggregateException(maxLevel, logEntries, aggreateId, currentLevel, aggregateException);
                return;
            }

            FillLogEntries(exception.InnerException, maxLevel, logEntries, aggreateId, currentLevel + 1);
        }

        static void FillLogEntriesForAggregateException(int maxLevel, List<LogEntry> logEntries, Guid aggreateId, int currentLevel,
            AggregateException aggregateException)
        {
            foreach (var ex in aggregateException.InnerExceptions)
            {
                if (ex == null)
                {
                    continue;
                }
                FillLogEntries(ex, maxLevel, logEntries, aggreateId, currentLevel + 1);
            }
        }

        private static bool IsAllBranchesMarkedLogEntry(Exception exception, int maxLevel, bool logEntryFound = false, int currentLevel = 1)
        {
            if (exception == null) { return logEntryFound; }
            if (currentLevel > maxLevel) { return logEntryFound; }

            var aggregateException = exception as AggregateException;
            if (aggregateException != null && aggregateException.InnerExceptions.Any())
            {
                if (aggregateException.InnerExceptions.Any(ex => !IsAllBranchesMarkedLogEntry(ex, maxLevel, currentLevel: currentLevel + 1)))
                {
                    return false;
                }
                return true;
            }

            var logEntryObject = exception.Data[LOG_ENTRY_KEY];
            var logEntry = logEntryObject as LogEntryMark;
            if (logEntry != null)
            {
                logEntryFound = true;
            }
            return IsAllBranchesMarkedLogEntry(exception.InnerException, maxLevel, logEntryFound, currentLevel + 1);
        }


        private static AxeLogLevel GetLogLevel(LogEntryMark logEntry)
        {
            switch (logEntry.Level)
            {
                case Level.DefinedByBusiness:
                    return AxeLogLevel.Info;
                case Level.IKnowItWillHappen:
                    return AxeLogLevel.Warn;
                case Level.Unknown:
                    return AxeLogLevel.Error;
                default:
                    return AxeLogLevel.Error;
            }
        }
    }
}