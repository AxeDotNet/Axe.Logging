namespace Axe.Logging.Core
{
    public class AxeLogManger
    {
        public static IAxeLogger GetLogger(string name, AxeLogSetting axeLogSetting)
        {
            AxeLogSetting settings = axeLogSetting ?? AxeLogSetting.Default;
            return settings.LoggingBackend.GetLogger(name);
        }

        public static IAxeLogger GetLogger(string name)
        {
            return GetLogger(name, null);
        }
    }
}