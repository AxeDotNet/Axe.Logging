using System;

namespace Axe.Logging.Core
{
    [Serializable]
    public class LogEntryMark
    {
        public LogEntryMark(DateTime time, object data, Level level)
        {
            Time = time;
            Data = data;
            Level = level;
        }

        public DateTime Time { get; }
        public object Data { get; }
        public Level Level { get; }
    }
}
