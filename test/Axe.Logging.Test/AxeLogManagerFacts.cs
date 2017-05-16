using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.Test
{
    public class AxeLogManagerFacts
    {
        [Fact]
        public void should_return_dummy_logger_when_not_set_log_backend()
        {
            var axeLogger = AxeLogManger.GetLogger("axeLogger");
            Assert.Equal(typeof(DummyLogger).Name, axeLogger.GetType().Name);
        }

        [Fact]
        public void should_return_nlog_logger_when_use_nlog_backend()
        {
            AxeLogSetting axeLogSetting = new AxeLogSetting() {LoggingBackend = "NLog"};
            var axeLogger = AxeLogManger.GetLogger("axeLogger", axeLogSetting);

            Assert.Equal(typeof(NLogLogger).Name, axeLogger.GetType().Name);
        }

        [Fact]
        public void should_return_nlog_logger_for_current_class_when_get_current_class_logger()
        {
           var currentClassLogger = AxeLogManger.GetCurrentClassLogger();

            Assert.Equal(typeof(DummyLogger).Name, currentClassLogger.GetType().Name);
            Assert.Equal("Axe.Logging.Test.AxeLogManagerFacts", ((DummyLogger)currentClassLogger).Name);
        }
    }
}