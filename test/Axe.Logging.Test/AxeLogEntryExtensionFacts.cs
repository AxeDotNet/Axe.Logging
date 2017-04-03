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
            LogEntry logEntry = CreateLogEntry();
            Exception exception = new Exception().Mark(logEntry);

            Assert.Equal(logEntry, exception.GetLogEntry().Single());
        }

        [Fact]
        public void should_throw_argumet_null_exception_when_mark_exception_given_exception_is_null()
        {
            Exception exception = null;
            LogEntry logEntry = CreateLogEntry();

            Assert.Throws<ArgumentNullException>(() => exception.Mark(logEntry));
        }

        [Fact]
        public void should_throw_argumet_null_exception_when_mark_exception_given_log_entry_is_null()
        {
            var exception = new Exception();

            Assert.Throws<ArgumentNullException>(() => exception.Mark(null));
        }

        [Fact]
        public void should_replace_the_marked_log_entry_when_mark_a_exception_already_marked()
        {
            LogEntry oldLogEntry = CreateLogEntry();
            LogEntry newLogEntry = CreateLogEntry();

            Exception exception = new Exception().Mark(oldLogEntry);
            exception.Mark(newLogEntry);

            Assert.Equal(newLogEntry, exception.GetLogEntry().Single());
        }

        [Fact]
        public void should_get_empty_log_entry_collection_when_exception_is_null()
        {
            Exception exception = null;

            Assert.Empty(exception.GetLogEntry());
        }

        [Fact]
        public void should_get_unknown_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_with_log_entry()
        {
            var exception = new Exception();

            LogEntry logEntry = exception.GetLogEntry().Single();

            Assert.Equal(DateTime.UtcNow, logEntry.Time);
            Assert.Equal(exception, logEntry.Data);
            Assert.Equal(Level.Unknown, logEntry.Level);
        }

        [Fact]
        public void should_get_log_entry_of_exception_if_log_entry_existed_in_inner_exception()
        {
            LogEntry logEntry = CreateLogEntry();
            Exception innerExceptionWithLogEntry = new Exception().Mark(logEntry);
            var exception = new Exception("parent", innerExceptionWithLogEntry);

            Assert.Equal(logEntry, exception.GetLogEntry().Single());
        }

        [Fact]
        public void should_get_all_log_entries_with_same_id_for_multiple_marked_exceptions()
        {
            LogEntry logEntryOnParent = CreateLogEntry();
            LogEntry logEntryOnInner = CreateLogEntry();

            Exception innerException = new Exception("inner").Mark(logEntryOnInner);
            Exception parentException = new Exception("parent", innerException).Mark(logEntryOnParent);

            LogEntry[] logEntries = parentException.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.Equal(logEntryOnParent, logEntries[0]);
            Assert.Equal(logEntryOnInner, logEntries[1]);
            Assert.True(logEntries[0].Id == logEntries[1].Id);
        }

        [Fact]
        public void should_get_all_log_entries_with_same_id_for_multiple_marked_aggregated_exceptions()
        {
            LogEntry logEntry1 = CreateLogEntry();
            LogEntry logEntry2 = CreateLogEntry();

            Exception innerExceptionMarkedOne = new Exception("inner exception one").Mark(logEntry1);
            Exception innerExceptionMarkedTwo = new Exception("inner exception two").Mark(logEntry2);
            var exception = new AggregateException(
                "aggregate exceptions", 
                innerExceptionMarkedOne, 
                innerExceptionMarkedTwo);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.True(logEntries.Contains(logEntry1));
            Assert.True(logEntries.Contains(logEntry2));
            Assert.True(logEntries[0].Id == logEntries[1].Id);
        }

        [Fact]
        public void should_get_one_default_log_entry_given_aggregate_exception_with_all_inner_exceptions_not_marked()
        {
            var notMarkedExceptionOne = new Exception("inner exception one");
            var notMarkedExceptionTwo = new Exception("inner exception two");
            var exception = new AggregateException(
                "aggregate exceptions", 
                notMarkedExceptionOne, 
                notMarkedExceptionTwo);

            LogEntry logEntry = exception.GetLogEntry().Single();

            Assert.Equal(Level.Unknown, logEntry.Level);
            Assert.Equal(exception, logEntry.Data);
        }

        [Fact]
        public void should_get_unknown_log_entries_when_not_all_branches_marked()
        {
            LogEntry logEntry = CreateLogEntry();
            var notMarkedException = new Exception();
            var markedException = new Exception().Mark(logEntry);
            var exception = new AggregateException("aggregate exceptions", notMarkedException, markedException);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);
            Assert.True(logEntries.Contains(logEntry));

            var notMarkedEntry = logEntries.Single(e => e.Level == Level.Unknown);
            Assert.Equal(exception, notMarkedEntry.Data);

            Assert.True(logEntries[0].Id == logEntries[1].Id);
        }

        [Fact]
        public void should_return_unknown_log_entry_given_aggregateException_has_no_inner_exceptions()
        {
            var exception = new AggregateException();

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(1, logEntries.Length);
            LogEntry retrivedLogEntry = logEntries.Single();
            
            Assert.Equal(Level.Unknown, retrivedLogEntry.Level);
            Assert.Equal(exception, retrivedLogEntry.Data);
        }

        [Fact]
        public void should_get_unknown_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_within_maxlevel()
        {
            LogEntry logEntry = CreateLogEntry();
            var exception = new Exception(
                "exception level 1", 
                new Exception("exception level 2").Mark(logEntry));

            LogEntry retrived = exception.GetLogEntry(1).Single();

            Assert.Equal(Level.Unknown, retrived.Level);
            Assert.Equal(exception, retrived.Data);
        }

        [Fact]
        public void should_get_log_entries_with_exceptions_marked_in_the_maxlevel()
        {
            LogEntry logEntry = CreateLogEntry();

            var aggregateException = new Exception(
                "level 1", 
                new Exception(
                    "level 2",
                    new Exception(
                        "level 3",
                        new Exception("level 4").Mark(logEntry))));

            LogEntry retrived = aggregateException.GetLogEntry(4).Single();

            Assert.Equal(logEntry, retrived);
            Assert.Equal(logEntry.Level, retrived.Level);
        }

        [Fact]
        public void should_get_log_entries_for_exception_contains_aggregate_exception()
        {
            LogEntry logEntry41 = CreateLogEntry();
            LogEntry logEntry52 = CreateLogEntry();
            LogEntry logEntry2 = CreateLogEntry();

            var exception = new Exception(
                "level 1",
                new Exception("level 2",
                        new AggregateException("level 3",
                            new Exception(
                                "level 4.1",
                                new Exception("level 5.1")).Mark(logEntry41),
                            new Exception(
                                "level 4.2",
                                new Exception("level 5.2").Mark(logEntry52))))
                    .Mark(logEntry2));

            LogEntry[] logEntries = exception.GetLogEntry(6);

            Assert.Equal(3, logEntries.Length);
            Assert.Contains(logEntry2, logEntries);
            Assert.Contains(logEntry41, logEntries);
            Assert.Contains(logEntry52, logEntries);

            Assert.True(logEntries.Select(e => e.Id).Distinct().Count() == 1);
        }

        static LogEntry CreateLogEntry()
        {
            var logEntry = new LogEntry(Guid.NewGuid(), DateTime.UtcNow, "This is Entry", new { Id = 1 }, new { Country = "China" }, Level.DefinedByBusiness);
            return logEntry;
        }
    }
}
