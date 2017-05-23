using System;

namespace Axe.Logging.Core.NullValues
{
    public class DummyLogger : IAxeLogger
    {
        public string Name { get; }

        public DummyLogger(string name)
        {
            Name = name;
        }

        public void Log(AxeLogLevel axeLogLevel, object data)
        {
        }

        public void Log(Exception exception)
        {
        }

        public void Info(object data)
        {
        }

        public void Error(object data)
        {
        }

        public void Warn(object data)
        {
        }
    }
}