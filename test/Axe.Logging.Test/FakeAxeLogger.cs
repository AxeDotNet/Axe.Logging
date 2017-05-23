using System.Collections.Generic;
using Axe.Logging.Core;
using Newtonsoft.Json;

namespace Axe.Logging.Test
{
    class FakeAxeLogger : LoggerBase
    {
        public List<LogEntry> Logs { get; } = new List<LogEntry>();
        
        protected override void WriteLog(AxeLogLevel axeLogLevel, string logMessage)
        {
            Logs.Add(JsonConvert.DeserializeObject<LogEntry>(logMessage));
        }
    }
}