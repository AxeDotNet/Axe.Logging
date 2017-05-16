namespace Axe.Logging.Core
{
    public static class AxeLogSettingExtension
    {
        public static void UseNLogBackend(this AxeLogSetting axeLogSetting) { axeLogSetting.LoggingBackend = new NLogLoggingBackend(); }
    }
}