using System.Collections.Generic;
using Axe.Logging.Core;
using Newtonsoft.Json;

namespace Axe.Logging.Test
{
    class FakeAxeLogger : LoggerBase
    {
        public List<LogEntry> Logs { get; } = new List<LogEntry>();
        
        protected override void WriteLog(AxeLogLevel level, string logMessage)
        {
            var entry = JsonConvert.DeserializeObject<LogEntry>(logMessage);
            Logs.Add(new LogEntry(entry.AggregateId, entry.Time, entry.Data, level));
        }
    }
}