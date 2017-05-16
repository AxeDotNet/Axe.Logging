namespace Axe.Logging.Core
{
    internal class DummyLoggingBackend : ILoggingBackend
    {
        public IAxeLogger GetLogger(string name)
        {
            return  new DummyLogger(name);
        }
    }
}