namespace Axe.Logging.Core
{
    public class DummyLoggingBackend : ILoggingBackend
    {
        public IAxeLogger GetLogger(string name)
        {
            return  new DummyLogger(name);
        }
    }
}