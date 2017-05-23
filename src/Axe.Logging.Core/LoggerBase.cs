using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Axe.Logging.Core
{
    public abstract class LoggerBase : IAxeLogger
    {
        static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public void Log(AxeLogLevel axeLogLevel, object data)
        {
            var entry = new LogEntry(Guid.NewGuid(), DateTime.UtcNow, data, axeLogLevel);
            WriteLog(entry);
        }

        public void Log(Exception exception)
        {
            LogEntry[] entries = exception.GetLogEntries();
            foreach (LogEntry entry in entries)
            {
                WriteLog(entry);
            }
        }

        public void Info(object data) => Log(AxeLogLevel.Info, data);

        public void Error(object data) => Log(AxeLogLevel.Error, data);

        public void Warn(object data) => Log(AxeLogLevel.Warn, data);

        void WriteLog(LogEntry entry)
        {
            WriteLog(entry.Level, JsonConvert.SerializeObject(entry, settings));
        }

        protected abstract void WriteLog(AxeLogLevel level, string logMessage);
    }
}