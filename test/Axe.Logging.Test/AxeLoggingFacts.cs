using System;
using System.Linq;
using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.Test
{
    public class AxeLoggingFacts
    {
        [Fact]
        public void should_get_log_entry_from_any_marked_exception()
        {
            LogEntry doNotCare = CreateLogEntry();
            Exception exception = new Exception().Mark(doNotCare);

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
            Exception innerExceptionWithLogEntry = new Exception().Mark(doNotCare);
            var exception = new Exception("do not care exception One", new Exception("do not care exception Two", innerExceptionWithLogEntry));

            Assert.Equal(doNotCare, exception.GetLogEntry().Single());
        }

        [Fact]
        public void should_get_all_log_entries_given_linked_list_type_exception_with_multiple_marked_exceptions()
        {
            LogEntry doNotCareLogEntryOne = CreateLogEntry();
            LogEntry doNotCareLogEntryTwo = CreateLogEntry();

            Exception innerException = new Exception("inner exception").Mark(doNotCareLogEntryTwo);
            Exception exception = new Exception("exception One", innerException).Mark(doNotCareLogEntryOne);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.Equal(doNotCareLogEntryOne, logEntries[0]);
            Assert.Equal(doNotCareLogEntryTwo, logEntries[1]);
        }

        [Fact]
        public void should_get_all_log_entries_given_aggregate_exception_with_multiple_marked_exceptions()
        {
            LogEntry doNotCareLogEntryOne = CreateLogEntry();
            LogEntry doNotCareLogEntryTwo = CreateLogEntry();

            Exception innerExceptionMarkedOne = new Exception("inner exception one").Mark(doNotCareLogEntryOne);
            Exception innerExceptionMarkedTwo = new Exception("inner exception two").Mark(doNotCareLogEntryTwo);
            var exception = new AggregateException("aggregate exceptions", innerExceptionMarkedOne, innerExceptionMarkedTwo);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.Equal(doNotCareLogEntryOne, logEntries[0]);
            Assert.Equal(doNotCareLogEntryTwo, logEntries[1]);
        }

        [Fact]
        public void should_get_one_default_log_entry_given_aggregate_exception_with_all_inner_exceptions_not_marked()
        {
            var innerExceptionNotMarkedOne = new Exception("inner exception one");
            var innerExceptionNotMarkedTwo = new Exception("inner exception two");
            var exception = new AggregateException("aggregate exceptions", innerExceptionNotMarkedOne, innerExceptionNotMarkedTwo);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(Level.Unknown, logEntries.Single().Level);
        }

        private static LogEntry CreateLogEntry()
        {
            var logEntry = new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "This is Entry", new { Id = 1 }, new { Country = "China" }, Level.DefinedByBusiness);
            return logEntry;
        }
    }

}
