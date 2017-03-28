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

        [Fact]
        public void shold_throw_argumet_null_exception_when_mark_exception_given_exception_is_null()
        {
            Exception exception = null;
            LogEntry doNotCare = CreateLogEntry();

            Assert.Throws<ArgumentNullException>(() => exception.Mark(doNotCare));
        }

        [Fact]
        public void shold_throw_argumet_null_exception_when_get_log_entry_from_a_null_exception()
        {
            Exception exception = null;

            Assert.Throws<ArgumentNullException>(() => exception.GetLogEntry());
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
