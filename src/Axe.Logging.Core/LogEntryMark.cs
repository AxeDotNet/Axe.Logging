using System;

namespace Axe.Logging.Core
{
    [Serializable]
    public class LogEntryMark
    {
        public LogEntryMark(DateTime time, string entry, object user, object data, Level level)
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
}
