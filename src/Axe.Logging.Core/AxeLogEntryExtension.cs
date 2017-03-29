using System;
using System.Collections.Generic;

namespace Axe.Logging.Core
{
    public static class AxeLogEntryExtension
    {
        private const string LOG_ENTRY_KEY = "Axe_Logging";

        public static T Mark<T>(this T exception, LogEntry logEntry) where T : Exception
        {
            VerifyException(exception);

            exception.Data.Add(LOG_ENTRY_KEY, logEntry);
            return exception;
        }

        public static LogEntry[] GetLogEntry(this Exception exception, int maxLevel = 10)
        {
            VerifyException(exception);

            var logEntries = new List<LogEntry>();

            if (!GetLogEntryFromException(exception, maxLevel, logEntries))
            {
                LogEntry defaultUknownException = CreateDefaultUknownException(exception);
                logEntries.Add(defaultUknownException);
            }

            return logEntries.ToArray();
        }

        private static void VerifyException<T>(T exception) where T : Exception
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }
        }

        private static LogEntry CreateDefaultUknownException(Exception exception)
        {
            return new LogEntry(Guid.Empty, DateTime.UtcNow, exception.Message, default(object), exception, Level.Unknown);
        }

        private static bool GetLogEntryFromException(Exception exception, int maxLevel, List<LogEntry> logEntries, bool isMarked = false, int currentLevel = 1)
        {
            if (currentLevel <= maxLevel)
            {
                if (exception.Data[LOG_ENTRY_KEY] != null)
                {
                    logEntries.Add(exception.Data[LOG_ENTRY_KEY] as LogEntry);
                    isMarked = true;
                }

                var aggregateException = exception as AggregateException;

                if (aggregateException != null)
                {
                    var isGetLogEntryFromException = true;
                    foreach (Exception ex in aggregateException.InnerExceptions)
                    {
                        if (ex != null)
                        {
                            if (!GetLogEntryFromException(ex, maxLevel, logEntries, currentLevel: currentLevel + 1)) isGetLogEntryFromException = false;
                        }

                    }
                    return isGetLogEntryFromException;
                }

                if (exception.InnerException != null)
                {
                    return GetLogEntryFromException(exception.InnerException, maxLevel, logEntries, isMarked, currentLevel + 1);
                }
            }
            return isMarked;
        }
    }
}