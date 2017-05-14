using System;

namespace Axe.Logging.Core
{
    public class LogEntry
    {
        public LogEntry(Guid aggregateId, DateTime time, object data, LogLevel level)
        {
            AggregateId = aggregateId;
            Time = time;
            Data = data;
            Level = level;
        }

        public Guid AggregateId { get; set; }
        public DateTime Time { get; }
        public object Data { get; }
        public LogLevel Level { get; }
    }
}