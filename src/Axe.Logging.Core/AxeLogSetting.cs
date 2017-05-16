namespace Axe.Logging.Core
{
    public class AxeLogSetting
    {
        public static AxeLogSetting Default { get;} = new AxeLogSetting();

        public string LoggingBackend { get; set; } = string.Empty;

        public void UseNLogBackend() { LoggingBackend = "NLog"; }

    }
}