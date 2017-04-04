using System;
using System.Linq;
using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.Test
{
    public class AxeLogEntryExtensionFacts
    {
        [Theory]
        [InlineData(Level.DefinedByBusiness, LogLevel.Info)]
        [InlineData(Level.IKnowItWillHappen, LogLevel.Warning)]
        [InlineData(Level.Unknown, LogLevel.Error)]
        public void should_get_log_entry_from_any_marked_exception(Level levelMarked, LogLevel loglevel)
        {
            LogEntryMark logEntry = CreateLogEntry(levelMarked);
            Exception exception = new Exception().Mark(logEntry);

            LogEntry logEntryResult = exception.GetLogEntry().Single();

            VerifyLogEntry(logEntry, logEntryResult, loglevel);
        }

        [Fact]
        public void should_throw_argumet_null_exception_when_mark_exception_given_exception_is_null()
        {
            Exception exception = null;
            LogEntryMark logEntry = CreateLogEntry();

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
            LogEntryMark oldLogEntry = CreateLogEntry();
            LogEntryMark newLogEntry = CreateLogEntry();

            Exception exception = new Exception().Mark(oldLogEntry);
            exception.Mark(newLogEntry);

            VerifyLogEntry(newLogEntry, exception.GetLogEntry().Single());
        }

        [Fact]
        public void should_get_empty_log_entry_collection_when_exception_is_null()
        {
            Exception exception = null;

            Assert.Empty(exception.GetLogEntry());
        }

        [Fact]
        public void should_get_error_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_with_log_entry()
        {
            var exception = new Exception();

            LogEntry logEntry = exception.GetLogEntry().Single();

            Assert.Equal(DateTime.UtcNow, logEntry.Time);
            Assert.Equal(exception, logEntry.Data);
            Assert.Equal(LogLevel.Error, logEntry.Level);
        }

        [Fact]
        public void should_get_log_entry_of_exception_if_log_entry_existed_in_inner_exception()
        {
            LogEntryMark logEntry = CreateLogEntry();
            Exception innerExceptionWithLogEntry = new Exception().Mark(logEntry);
            var exception = new Exception("parent", innerExceptionWithLogEntry);

            LogEntry logEntryResult = exception.GetLogEntry().Single();

            VerifyLogEntry(logEntry, logEntryResult);
        }

        [Fact]
        public void should_get_all_log_entries_with_same_id_for_multiple_marked_exceptions()
        {
            LogEntryMark logEntryOnParent = CreateLogEntry();
            LogEntryMark logEntryOnInner = CreateLogEntry();

            Exception innerException = new Exception("inner").Mark(logEntryOnInner);
            Exception parentException = new Exception("parent", innerException).Mark(logEntryOnParent);

            LogEntry[] logEntries = parentException.GetLogEntry();

            Assert.Equal(2, logEntries.Length);

            VerifyLogEntry(logEntryOnParent, logEntries[0]);
            VerifyLogEntry(logEntryOnInner, logEntries[1]);

            Assert.True(logEntries[0].AggregateId == logEntries[1].AggregateId);
        }

        [Fact]
        public void should_get_all_log_entries_with_same_id_for_multiple_marked_aggregated_exceptions()
        {
            LogEntryMark logEntry1 = CreateLogEntry(Level.DefinedByBusiness);
            LogEntryMark logEntry2 = CreateLogEntry(Level.IKnowItWillHappen);

            Exception innerExceptionMarkedOne = new Exception("inner exception one").Mark(logEntry1);
            Exception innerExceptionMarkedTwo = new Exception("inner exception two").Mark(logEntry2);
            var exception = new AggregateException(
                "aggregate exceptions", 
                innerExceptionMarkedOne, 
                innerExceptionMarkedTwo);

            LogEntry[] logEntries = exception.GetLogEntry();
            var innerExceptionMarkedOneResult = logEntries.Single(l => l.Level == LogLevel.Info);
            var innerExceptionMarkedTwoResult = logEntries.Single(l => l.Level == LogLevel.Warning);

            Assert.Equal(2, logEntries.Length);

            VerifyLogEntry(logEntry1, innerExceptionMarkedOneResult);
            VerifyLogEntry(logEntry2, innerExceptionMarkedTwoResult, LogLevel.Warning);

            Assert.True(logEntries[0].AggregateId == logEntries[1].AggregateId);
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

            Assert.Equal(LogLevel.Error, logEntry.Level);
            Assert.Equal(exception, logEntry.Data);
        }

        [Fact]
        public void should_get_error_log_entries_when_not_all_branches_marked()
        {
            LogEntryMark logEntry = CreateLogEntry();
            var notMarkedException = new Exception();
            var markedException = new Exception().Mark(logEntry);
            var exception = new AggregateException("aggregate exceptions", notMarkedException, markedException);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);

            var markedEntry = logEntries.Single(e => e.Level == LogLevel.Info);
            VerifyLogEntry(logEntry, markedEntry);

            var notMarkedEntry = logEntries.Single(e => e.Level == LogLevel.Error);
            Assert.Equal(exception, notMarkedEntry.Data);

            Assert.True(logEntries[0].AggregateId == logEntries[1].AggregateId);
        }

        [Fact]
        public void should_return_error_log_entry_given_aggregateException_has_no_inner_exceptions()
        {
            var exception = new AggregateException();

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(1, logEntries.Length);
            LogEntry retrivedLogEntry = logEntries.Single();
            
            Assert.Equal(LogLevel.Error, retrivedLogEntry.Level);
            Assert.Equal(exception, retrivedLogEntry.Data);
        }

        [Fact]
        public void should_get_error_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_within_maxlevel()
        {
            LogEntryMark logEntry = CreateLogEntry();
            var exception = new Exception(
                "exception level 1", 
                new Exception("exception level 2").Mark(logEntry));

            LogEntry retrived = exception.GetLogEntry(1).Single();

            Assert.Equal(LogLevel.Error, retrived.Level);
            Assert.Equal(exception, retrived.Data);
        }

        [Fact]
        public void should_get_log_entries_with_exceptions_marked_in_the_maxlevel()
        {
            LogEntryMark logEntry = CreateLogEntry();

            var aggregateException = new Exception(
                "level 1", 
                new Exception(
                    "level 2",
                    new Exception(
                        "level 3",
                        new Exception("level 4").Mark(logEntry))));

            LogEntry retrived = aggregateException.GetLogEntry(4).Single();
            
            VerifyLogEntry(logEntry, retrived);
        }

        [Fact]
        public void should_get_log_entries_for_exception_contains_aggregate_exception()
        {
            LogEntryMark logEntry41 = CreateLogEntry(Level.DefinedByBusiness);
            LogEntryMark logEntry52 = CreateLogEntry(Level.IKnowItWillHappen);
            LogEntryMark logEntry2 = CreateLogEntry(Level.Unknown);

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
            var logEntry41Result = logEntries.Single(e => e.Level == LogLevel.Info);
            var logEntry52Result = logEntries.Single(e => e.Level == LogLevel.Warning);
            var logEntry2Result = logEntries.Single(e => e.Level == LogLevel.Error);

            Assert.Equal(3, logEntries.Length);

            VerifyLogEntry(logEntry2, logEntry2Result, LogLevel.Error);
            VerifyLogEntry(logEntry41, logEntry41Result, LogLevel.Info);
            VerifyLogEntry(logEntry52, logEntry52Result, LogLevel.Warning);

            Assert.True(logEntries.Select(e => e.AggregateId).Distinct().Count() == 1);
        }

        private static void VerifyLogEntry(LogEntryMark logEntryMarked, LogEntry logEntryResult, LogLevel logLevel = LogLevel.Info)
        {
            Assert.Equal(logEntryMarked.Time, logEntryResult.Time);
            Assert.Equal(logEntryMarked.Entry, logEntryResult.Entry);
            Assert.Equal(logEntryMarked.User, logEntryResult.User);
            Assert.Equal(logEntryMarked.Data, logEntryResult.Data);
            Assert.Equal(logLevel, logEntryResult.Level);
        }

        static LogEntryMark CreateLogEntry(Level levelMarked = Level.DefinedByBusiness)
        {
            var logEntry = new LogEntryMark(DateTime.UtcNow, "This is Entry", new { Id = 1 }, new { Country = "China" }, levelMarked);
            return logEntry;
        }
    }
}
