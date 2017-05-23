using System;

namespace Axe.Logging.Core
{
    [Serializable]
    class LogEntryMark
    {
        public LogEntryMark(DateTime time, object data, AxeLogLevel level)
        {
            Time = time;
            Data = data;
            Level = level;
        }

        public DateTime Time { get; }
        public object Data { get; }
        public AxeLogLevel Level { get; }
    }
}
