using System;

namespace Axe.Logging.Core
{
    [Serializable]
    public class LogEntry
    {
        public LogEntry(Guid id, DateTime time, string entry, object user, object data, Level level)
        {
            Id = id;
            Time = time;
            Entry = entry;
            User = user;
            Data = data;
            Level = level;
        }

        public Guid Id { get; }
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
