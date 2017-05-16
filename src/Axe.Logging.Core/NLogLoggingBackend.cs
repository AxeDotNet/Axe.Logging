namespace Axe.Logging.Core
{
    public class NLogLoggingBackend : ILoggingBackend
    {
        public IAxeLogger GetLogger(string name)
        {
            return new NLogLogger(name);
        }
    }
}