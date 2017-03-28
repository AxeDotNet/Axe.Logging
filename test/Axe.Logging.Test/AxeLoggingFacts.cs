using System;
using System.Linq;
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

            Assert.Equal(doNotCare, exception.GetLogEntry().Single());
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

            LogEntry logEntry = exception.GetLogEntry().Single();

            Assert.Equal(DateTime.UtcNow, logEntry.Time);
            Assert.Equal(exception.Message, logEntry.Entry);
            Assert.Equal(exception, logEntry.Data);
            Assert.Equal(Level.Unknown, logEntry.Level);
        }

        [Fact]
        public void should_get_log_entry_of_exception_given_linked_type_exception_and_log_entry_existed_in_inner_exception()
        {
            LogEntry doNotCare = CreateLogEntry();
            var innerExceptionWithLogEntry = new Exception().Mark(doNotCare);
            var exception = new Exception("edo not care exception 1", new Exception("do not care exception 2", innerExceptionWithLogEntry));

            Assert.Equal(doNotCare, exception.GetLogEntry().Single());
        }

        [Fact]
        public void should_get_all_log_entries_given_linked_list_type_exception_with_multiple_marked_exceptions()
        {
            var id = Guid.NewGuid();
            LogEntry doNotCare1 = CreateLogEntry();
            LogEntry doNotCare2 = CreateLogEntry();

            var innerException = new Exception("inner exception").Mark(doNotCare2);
            var exception = new Exception("exception 1", innerException).Mark(doNotCare1);

            var logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.Equal(doNotCare1, logEntries[0]);
            Assert.Equal(doNotCare2, logEntries[1]);
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
