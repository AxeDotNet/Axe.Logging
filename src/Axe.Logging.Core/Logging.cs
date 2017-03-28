using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Axe.Logging.Core
{
    public static class Logging
    {
        private const string LOG_ENTRY_KEY = "Axe_Logging";

        public static T Mark<T>(this T exception, LogEntry logEntry) where T : Exception 
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            exception.Data.Add(LOG_ENTRY_KEY, logEntry);
            return exception;
        }

        public static LogEntry[] GetLogEntry(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            List<LogEntry> logEntries = new List<LogEntry>();
            GetLogEntryFromException(exception, ref logEntries);

            if (logEntries.Count == 0)
            {
                var defaultUknownException = CreateDefaultUknownException(exception);
                logEntries.Add(defaultUknownException);
            }

            return logEntries.ToArray();
        }

        private static LogEntry CreateDefaultUknownException(Exception exception)
        {
            return new LogEntry(DateTime.UtcNow, exception.Message, default(object), exception, Level.Unknown);
        }

        [SuppressMessage("ReSharper", "UseNullPropagation")]
        private static void GetLogEntryFromException(Exception exception, ref List<LogEntry> logEntries)
        {
            if (exception.Data[LOG_ENTRY_KEY] != null)
            {
                logEntries.Add(exception.Data[LOG_ENTRY_KEY] as LogEntry);
            }

            if (exception.InnerException != null)
            {
                GetLogEntryFromException(exception.InnerException, ref logEntries);
            }
        }
    }

    [Serializable]
    public class LogEntry
    {
        public LogEntry(DateTime time, string entry, object user, object data, Level level)
        {
            Time = time;
            Entry = entry;
            User = user;
            Data = data;
            Level = level;
        }

        public DateTime Time { get; }
        public string Entry { get; }
        public object User { get; }
        public object Data { get; }
        public Level Level { get; }
    }


    public enum Level
    {
        DefinedByBusiness,
        IKnowItWillHappen,
        Unknown
    }
}
