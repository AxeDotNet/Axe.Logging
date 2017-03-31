using System;
using System.Collections.Generic;
using System.Linq;

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

            if (FillLogEntries(exception, maxLevel, logEntries))
            {
                return logEntries.ToArray();
            }

            LogEntry defaultUknownException = CreateDefaultUknownException(exception);
            logEntries.Add(defaultUknownException);

            return logEntries.ToArray();
        }

        private static void VerifyException(Exception exception)
        {
            if (exception != null) return;
            throw new ArgumentNullException(nameof(exception));
        }

        private static LogEntry CreateDefaultUknownException(Exception exception)
        {
            return new LogEntry(Guid.Empty, DateTime.UtcNow, exception.Message, null, exception, Level.Unknown);
        }

        private static bool FillLogEntries(Exception exception, int maxLevel, List<LogEntry> logEntries, bool markedExceptionFound = false, int currentLevel = 1)
        {
            if (currentLevel > maxLevel) { return markedExceptionFound; }

            var logEntryObject = exception.Data[LOG_ENTRY_KEY];
            var logEntry = logEntryObject as LogEntry;
            if (logEntry != null)
            {
                logEntries.Add(logEntry);
                markedExceptionFound = true;
            }
            
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                bool isGetLogEntryFromException = aggregateException.InnerExceptions.Any();

                foreach (Exception ex in aggregateException.InnerExceptions)
                {
                    if (ex == null) { continue; }
                    if (!FillLogEntries(ex, maxLevel, logEntries, currentLevel: currentLevel + 1))
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

            return FillLogEntries(exception.InnerException, maxLevel, logEntries, markedExceptionFound, currentLevel + 1);
        }
    }
}