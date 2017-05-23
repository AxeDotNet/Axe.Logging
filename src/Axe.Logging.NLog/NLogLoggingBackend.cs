using Axe.Logging.Core;

namespace Axe.Logging.NLog
{
    class NLogLoggingBackend : ILoggingBackend
    {
        public IAxeLogger GetLogger(string name)
        {
            return new NLogLogger(name);
        }
    }
}