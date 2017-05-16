namespace Axe.Logging.Core
{
    public class AxeLogSetting
    {
        public static AxeLogSetting Default { get;} = new AxeLogSetting();

        public ILoggingBackend LoggingBackend { get; set; } = new DummyLoggingBackend();
    }
}