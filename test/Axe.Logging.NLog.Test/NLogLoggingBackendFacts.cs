using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.NLog.Test
{
    public class NLogLoggingBackendFacts
    {
        [Fact]
        public void should_return_nlog_logger_when_use_nlog_backend()
        {
            AxeLogSetting axeLogSetting = new AxeLogSetting();
            axeLogSetting.UseNLogBackend();
            var axeLogger = AxeLogManger.GetLogger("axeLogger", axeLogSetting);

            Assert.Equal(typeof(NLogLogger).Name, axeLogger.GetType().Name);
        }
    }
}
