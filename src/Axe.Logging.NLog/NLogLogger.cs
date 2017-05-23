using Axe.Logging.Core;
using NLog;

namespace Axe.Logging.NLog
{
    class NLogLogger : LoggerBase
    {
        readonly Logger logger;

        public NLogLogger(string name) { logger = LogManager.GetLogger(name); }

        protected override void WriteLog(AxeLogLevel axeLogLevel, string logMessage)
        {
            switch (axeLogLevel)
            {
                case AxeLogLevel.Info:
                    logger.Info(logMessage);
                    break;
                case AxeLogLevel.Warn:
                    logger.Warn(logMessage);
                    break;
                default:
                    logger.Error(logMessage);
                    break;
            }
        }
    }
}