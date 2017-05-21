using Axe.Logging.Core;

namespace Axe.Logging.NLog
{
    public static class NLogLoggingBackendExtension
    {
        public static void UseNLogBackend(this AxeLogSetting axeLogSetting)
        {
            axeLogSetting.LoggingBackend = new NLogLoggingBackend();
        }
    }
}