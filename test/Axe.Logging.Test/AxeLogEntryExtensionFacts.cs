using System;
using System.Linq;
using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.Test
{
    public class AxeLogEntryExtensionFacts
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
        public void should_throw_argumet_null_exception_when_mark_exception_given_log_entry_is_null()
        {
            Exception exception = new Exception();

            Assert.Throws<ArgumentNullException>(() => exception.Mark(null));
        }

        [Fact]
        public void should_replace_the_marked_log_entry_when_mark_a_exception_already_marked()
        {
            LogEntry oldLogEntry = CreateLogEntry();
            Exception exception = new Exception().Mark(oldLogEntry);
            LogEntry newLogEntry = CreateLogEntry();
            exception.Mark(newLogEntry);

            Assert.Equal(newLogEntry, exception.GetLogEntry().Single());
        }

        [Fact]
        public void should_get_empty_log_entry_collection_when_exception_is_null()
        {
            Exception exception = null;

            Assert.Equal(0, exception.GetLogEntry().Length);
        }

        [Fact]
        public void should_get_unknown_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_with_log_entry()
        {
            var exception = new Exception();

            LogEntry logEntry = exception.GetLogEntry().Single();

            Assert.Equal(DateTime.UtcNow, logEntry.Time);
            Assert.Equal(null, logEntry.Entry);
            Assert.Equal(null, logEntry.User);
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
        public void should_get_all_log_entries_and_with_same_id_given_linked_list_type_exception_with_multiple_marked_exceptions()
        {
            LogEntry doNotCareLogEntryOne = CreateLogEntry();
            LogEntry doNotCareLogEntryTwo = CreateLogEntry();

            Exception innerException = new Exception("inner exception").Mark(doNotCareLogEntryTwo);
            Exception exception = new Exception("exception One", innerException).Mark(doNotCareLogEntryOne);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.Equal(doNotCareLogEntryOne, logEntries[0]);
            Assert.Equal(doNotCareLogEntryTwo, logEntries[1]);
            Assert.True(logEntries[0].Id == logEntries[1].Id);
        }

        [Fact]
        public void should_get_all_log_entries_and_with_same_id_given_aggregate_exception_with_multiple_marked_exceptions()
        {
            LogEntry doNotCareLogEntryOne = CreateLogEntry();
            LogEntry doNotCareLogEntryTwo = CreateLogEntry();

            Exception innerExceptionMarkedOne = new Exception("inner exception one").Mark(doNotCareLogEntryOne);
            Exception innerExceptionMarkedTwo = new Exception("inner exception two").Mark(doNotCareLogEntryTwo);
            var exception = new AggregateException("aggregate exceptions", innerExceptionMarkedOne, innerExceptionMarkedTwo);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.True(logEntries.Contains(doNotCareLogEntryOne));
            Assert.True(logEntries.Contains(doNotCareLogEntryTwo));
            Assert.True(logEntries[0].Id == logEntries[1].Id);
        }

        [Fact]
        public void should_get_one_default_log_entry_given_aggregate_exception_with_all_inner_exceptions_not_marked()
        {
            var innerExceptionNotMarkedOne = new Exception("inner exception one");
            var innerExceptionNotMarkedTwo = new Exception("inner exception two");
            var exception = new AggregateException("aggregate exceptions", innerExceptionNotMarkedOne, innerExceptionNotMarkedTwo);

            LogEntry logEntry = exception.GetLogEntry().Single();

            Assert.Equal(Level.Unknown, logEntry.Level);
            Assert.Equal(exception, logEntry.Data);
        }

        [Fact]
        public void should_get_log_entries_with_one_default_and_id_should_be_same_with_other_log_engtries_given_aggregate_exception_with_one_inner_exception_marked_and_others_not_marked()
        {
            LogEntry doNotCareLogEntry = CreateLogEntry();
            var innerExceptionNotMarkedOne = new Exception("inner exception one", new Exception("inner exception one"));
            var innerExceptionMarkedTwo = new Exception("inner exception two", new Exception("inner exception two", new Exception("inner exception two")).Mark(doNotCareLogEntry));
            var exception = new AggregateException("aggregate exceptions", innerExceptionNotMarkedOne, innerExceptionMarkedTwo);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.Equal(doNotCareLogEntry, logEntries[0]);
            Assert.Equal(Level.Unknown, logEntries[1].Level);
            Assert.Equal(exception, logEntries[1].Data);

            Assert.True(logEntries[0].Id == logEntries[1].Id);
        }

        [Fact]
        public void should_return_Unknown_log_entry_given_aggregateException_has_no_inner_exceptions()
        {
            LogEntry doNotCare = CreateLogEntry();
            var exception = new Exception("exception", new AggregateException()).Mark(doNotCare);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.Equal(doNotCare, logEntries[0]);
            Assert.Equal(Level.Unknown, logEntries[1].Level);
            Assert.Equal(exception, logEntries[1].Data);

            Assert.True(logEntries[0].Id == logEntries[1].Id);
        }

        [Fact]
        public void should_get_unknown_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_with_log_entry_in_the_maxlevel()
        {
            LogEntry doNotCareLogEntry = CreateLogEntry();
            var exception = new Exception("exception level 1", new Exception("exception level 2").Mark(doNotCareLogEntry));

            LogEntry logEntry = exception.GetLogEntry(1).Single();

            Assert.Equal(Level.Unknown, logEntry.Level);
            Assert.Equal(exception, logEntry.Data);
        }

        [Fact]
        public void should_get_log_entries_with_exceptions_marked_in_the_maxlevel()
        {
            LogEntry doNotCareLogEntryOne = CreateLogEntry();
            LogEntry doNotCareLogEntryTwo = CreateLogEntry();

            Exception innerExceptionOne = new Exception("exception level 2", new Exception("exception level 3")).Mark(doNotCareLogEntryOne);
            var innerExceptionTwo = new Exception("exception level 2", new Exception("exception level 3", new Exception("exception level 4").Mark(doNotCareLogEntryTwo)));
            var aggregateException = new AggregateException(innerExceptionOne, innerExceptionTwo);

            LogEntry[] logEntries = aggregateException.GetLogEntry(2);

            Assert.Equal(doNotCareLogEntryOne, logEntries[0]);

            Assert.Equal(Level.Unknown, logEntries[1].Level);
            Assert.Equal(aggregateException, logEntries[1].Data);

            Assert.True(logEntries[0].Id == logEntries[1].Id);
        }

        [Fact]
        public void should_get_log_entries_for_exception_contains_aggregate_exception()
        {
            LogEntry doNotCareLogEntryOne = CreateLogEntry();
            LogEntry doNotCareLogEntryTwo = CreateLogEntry();
            LogEntry doNotCareLogEntryThree = CreateLogEntry();

            var innerExceptionOne = new Exception("exception level 4", new Exception("exception level 5")).Mark(doNotCareLogEntryOne);
            var innerExceptionTwo = new Exception("exception level 4", new Exception("exception level 5").Mark(doNotCareLogEntryTwo));
            var exception = new Exception("exception level 1", new Exception("exception level 2", new AggregateException("exception level 3", innerExceptionOne, innerExceptionTwo)).Mark(doNotCareLogEntryThree));

            LogEntry[] logEntries = exception.GetLogEntry(6);

            Assert.Equal(3, logEntries.Length);
            Assert.Equal(doNotCareLogEntryThree, logEntries[0]);
            Assert.True(logEntries.Contains(doNotCareLogEntryOne));
            Assert.True(logEntries.Contains(doNotCareLogEntryTwo));

            Assert.True(logEntries[0].Id == logEntries[1].Id);
            Assert.True(logEntries[0].Id == logEntries[2].Id);
        }

        private static LogEntry CreateLogEntry()
        {
            var logEntry = new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "This is Entry", new { Id = 1 }, new { Country = "China" }, Level.DefinedByBusiness);
            return logEntry;
        }
    }
}
