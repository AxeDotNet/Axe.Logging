using System;
using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.Test
{
    public class AxeLoggingFacts
    {
        [Fact]
        public void shold_get_log_entry_form_any_marked_exception()
        {
            LogEntry doNotCare = CreateLogEntry();
            var exception = new Exception().Mark(doNotCare);

            Assert.Equal(doNotCare, exception.GetLogEntry());
        }

        private static LogEntry CreateLogEntry()
        {
            var time = DateTime.UtcNow;
            const string entry = "This is Entry";
            var user = new { Id = 1 };
            var data = new { Country = "China" };
            var level = Level.DefinedByBusiness;
            return new LogEntry(time, entry, user, data, level);
        }
    }

}
