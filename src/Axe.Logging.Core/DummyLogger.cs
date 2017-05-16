namespace Axe.Logging.Core
{
    public class DummyLogger : LoggerBase
    {
        public string Name { get; }

        public DummyLogger(string name) { Name = name; }

        public override void RecordLogEntry(LogEntry logEntry)
        {
            
        }
    }
}