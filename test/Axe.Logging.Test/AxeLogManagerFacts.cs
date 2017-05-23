using Axe.Logging.Core;
using Axe.Logging.Core.NullValues;
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
    }
}