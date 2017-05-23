namespace Axe.Logging.Core.NullValues
{
    public class DummyLoggingBackend : ILoggingBackend
    {
        public IAxeLogger GetLogger(string name)
        {
            return new DummyLogger(name);
        }
    }
}