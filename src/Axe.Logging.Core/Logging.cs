using System;
using System.Collections.Generic;

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

        public static LogEntry GetLogEntry(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (exception.Data[LOG_ENTRY_KEY] != null)
            {
                return exception.Data[LOG_ENTRY_KEY] as LogEntry;
            }
            if (exception.InnerException != null)
            {
                return exception.InnerException.GetLogEntry();
            }
            return new LogEntry(DateTime.UtcNow, exception.Message, default(object), exception, Level.Unknown);
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
