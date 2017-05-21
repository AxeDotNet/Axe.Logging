using Axe.Logging.Core;

namespace Axe.Logging.NLog
{
    public class NLogLoggingBackend : ILoggingBackend
    {
        public IAxeLogger GetLogger(string name)
        {
            return new NLogLogger(name);
        }
    }
}