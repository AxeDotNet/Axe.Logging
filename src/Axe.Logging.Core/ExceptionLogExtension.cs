﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.Logging.Core
{
    public static class ExceptionLogExtension
    {
        const string LOG_ENTRY_KEY = "Axe_Logging";

        public static T MarkAsInfo<T>(this T exception, object data) where T : Exception
        {
            return MarkLogEntryForException(exception, AxeLogLevel.Info, data);
        }

        public static T MarkAsWarn<T>(this T exception, object data) where T : Exception
        {
            return MarkLogEntryForException(exception, AxeLogLevel.Warn, data);
        }

        public static T MarkAsError<T>(this T exception, object data) where T : Exception
        {
            return MarkLogEntryForException(exception, AxeLogLevel.Error, data);
        }

        internal static LogEntry[] GetLogEntries(this Exception exception, int maxLevel = 10)
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

        static T MarkLogEntryForException<T>(T exception, AxeLogLevel axeLogLevel, object data) where T : Exception
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var logEntry = new LogEntryMark(DateTime.UtcNow, data, axeLogLevel);

            if (exception.Data[LOG_ENTRY_KEY] != null)
            {
                exception.Data.Remove(LOG_ENTRY_KEY);
            }

            exception.Data.Add(LOG_ENTRY_KEY, logEntry);
            return exception;
        }

        static LogEntry CreateDefaultUknownException(Guid aggreateId, Exception exception)
        {
            return new LogEntry(aggreateId, DateTime.UtcNow, exception, AxeLogLevel.Error);
        }

        static void FillLogEntries(Exception exception, int maxLevel, List<LogEntry> logEntries, Guid aggreateId, int currentLevel = 1)
        {
            if(exception == null) return;
            if (currentLevel > maxLevel) return; 

            var logEntryObject = exception.Data[LOG_ENTRY_KEY];
            var logEntry = logEntryObject as LogEntryMark;
            if (logEntry != null)
            {
                var entry = new LogEntry(aggreateId, logEntry.Time, logEntry.Data, logEntry.Level);

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

        static bool IsAllBranchesMarkedLogEntry(Exception exception, int maxLevel, int currentLevel = 1)
        {
            if (exception == null) { return false; }
            if (currentLevel > maxLevel) { return false; }

            var logEntryObject = exception.Data[LOG_ENTRY_KEY];
            var logEntry = logEntryObject as LogEntryMark;
            if (logEntry != null)
            {
                return true;
            }

            var aggregateException = exception as AggregateException;
            if (aggregateException != null && aggregateException.InnerExceptions.Any())
            {
                if (aggregateException.InnerExceptions.Any(ex => !IsAllBranchesMarkedLogEntry(ex, maxLevel, currentLevel + 1)))
                {
                    return false;
                }
                return true;
            }

            return IsAllBranchesMarkedLogEntry(exception.InnerException, maxLevel, currentLevel + 1);
        }
    }
}