using System;
using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.Test
{
    public class AxeLoggingFacts
    {
        [Fact]
        public void should_get_log_entry_form_any_marked_exception()
        {
            LogEntry doNotCare = CreateLogEntry();
            var exception = new Exception().Mark(doNotCare);

            Assert.Equal(doNotCare, exception.GetLogEntry());
        }

        [Fact]
        public void should_throw_argumet_null_exception_when_mark_exception_given_exception_is_null()
        {
            Exception exception = null;
            LogEntry doNotCare = CreateLogEntry();

            Assert.Throws<ArgumentNullException>(() => exception.Mark(doNotCare));
        }

        [Fact]
        public void should_throw_argumet_null_exception_when_get_log_entry_from_a_null_exception()
        {
            Exception exception = null;

            Assert.Throws<ArgumentNullException>(() => exception.GetLogEntry());
        }

        [Fact]
        public void should_get_unknown_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_with_log_entry()
        {
            var exception = new Exception();

            LogEntry logEntry = exception.GetLogEntry();

            Assert.Equal(DateTime.UtcNow, logEntry.Time);
            Assert.Equal(exception.Message, logEntry.Entry);
            Assert.Equal(exception, logEntry.Data);
            Assert.Equal(Level.Unknown, logEntry.Level);
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
