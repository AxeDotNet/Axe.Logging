using System;
using Newtonsoft.Json;

namespace Axe.Logging.Core
{
    class LogEntry
    {
        public LogEntry(Guid aggregateId, DateTime time, object data, AxeLogLevel level)
        {
            AggregateId = aggregateId;
            Time = time;
            Data = data;
            Level = level;
        }

        public Guid AggregateId { get; set; }
        public DateTime Time { get; }
        public object Data { get; }

        [JsonIgnore]
        public AxeLogLevel Level { get; }
    }
}