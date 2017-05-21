using System;
using System.Linq;
using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.Test
{
    public class AxeLogEntryExtensionFacts
    {
        [Theory]
        [InlineData(AxeLogLevel.Info)]
        [InlineData(AxeLogLevel.Warn)]
        [InlineData(AxeLogLevel.Error)]
        public void should_get_log_entry_from_any_marked_exception(AxeLogLevel loglevel)
        {
            object logEntryData = CreateLogEntryData();
            Exception exception = new Exception().Mark(loglevel, logEntryData);

            LogEntry logEntryResult = exception.GetLogEntry().Single();

            VerifyLogEntry(logEntryResult, logEntryData, loglevel);
        }

        [Fact]
        public void should_throw_argumet_null_exception_when_mark_exception_given_exception_is_null()
        {
            Exception exception = null;
            object logEntryData = CreateLogEntryData();

            Assert.Throws<ArgumentNullException>(() => exception.Mark(AxeLogLevel.Info, logEntryData));
        }

        [Fact]
        public void should_replace_the_marked_log_entry_when_mark_a_exception_already_marked()
        {
            object oldLogEntryData = CreateLogEntryData("oldLogEntryData");
            object newLogEntryData = CreateLogEntryData("newLogEntryData");

            Exception exception = new Exception().Mark(AxeLogLevel.Info, oldLogEntryData);
            exception.Mark(AxeLogLevel.Error, newLogEntryData);

            VerifyLogEntry(exception.GetLogEntry().Single(), newLogEntryData, AxeLogLevel.Error);
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
            Assert.Equal(AxeLogLevel.Error, logEntry.Level);
        }

        [Fact]
        public void should_get_log_entry_of_exception_if_log_entry_existed_in_inner_exception()
        {
            object logEntryData = CreateLogEntryData();
            Exception innerExceptionWithLogEntry = new Exception().Mark(AxeLogLevel.Info, logEntryData);
            var exception = new Exception("parent", innerExceptionWithLogEntry);

            LogEntry logEntryResult = exception.GetLogEntry().Single();

            VerifyLogEntry(logEntryResult, logEntryData, AxeLogLevel.Info);
        }

        [Fact]
        public void should_get_all_log_entries_with_same_id_for_multiple_marked_exceptions()
        {
            object logEntryOnParentData = CreateLogEntryData("logEntryOnParentData");
            object logEntryOnInnerData = CreateLogEntryData("logEntryOnInnerData");

            Exception innerException = new Exception("inner").Mark(AxeLogLevel.Info, logEntryOnInnerData);
            Exception parentException = new Exception("parent", innerException).Mark(AxeLogLevel.Info, logEntryOnParentData);

            LogEntry[] logEntries = parentException.GetLogEntry();

            Assert.Equal(2, logEntries.Length);

            VerifyLogEntry(logEntries[0], logEntryOnParentData, AxeLogLevel.Info);
            VerifyLogEntry(logEntries[1], logEntryOnInnerData, AxeLogLevel.Info);

            Assert.True(logEntries[0].AggregateId == logEntries[1].AggregateId);
        }

        [Fact]
        public void should_get_all_log_entries_with_same_id_for_multiple_marked_aggregated_exceptions()
        {
            const AxeLogLevel axeLogLevel1 = AxeLogLevel.Info;
            object logEntryData1 = CreateLogEntryData("logEntry 1 Data");
            const AxeLogLevel axeLogLevel2 = AxeLogLevel.Warn;
            object logEntryData2 = CreateLogEntryData("logEntry 2 Data");

            Exception innerExceptionMarkedOne = new Exception("inner exception one").Mark(axeLogLevel1, logEntryData1);
            Exception innerExceptionMarkedTwo = new Exception("inner exception two").Mark(axeLogLevel2, logEntryData2);
            var exception = new AggregateException(
                "aggregate exceptions", 
                innerExceptionMarkedOne, 
                innerExceptionMarkedTwo);

            LogEntry[] logEntries = exception.GetLogEntry();
            var innerExceptionMarkedOneResult = logEntries.Single(l => l.Level == axeLogLevel1);
            var innerExceptionMarkedTwoResult = logEntries.Single(l => l.Level == axeLogLevel2);

            Assert.Equal(2, logEntries.Length);

            VerifyLogEntry(innerExceptionMarkedOneResult, logEntryData1, axeLogLevel1);
            VerifyLogEntry(innerExceptionMarkedTwoResult, logEntryData2, axeLogLevel2);

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

            Assert.Equal(AxeLogLevel.Error, logEntry.Level);
            Assert.Equal(exception, logEntry.Data);
        }

        [Fact]
        public void should_get_error_log_entries_when_not_all_branches_marked()
        {
            object logEntryData = CreateLogEntryData();
            var notMarkedException = new Exception();
            var markedException = new Exception().Mark(AxeLogLevel.Info, logEntryData);
            var exception = new AggregateException("aggregate exceptions", notMarkedException, markedException);

            LogEntry[] logEntries = exception.GetLogEntry();

            Assert.Equal(2, logEntries.Length);

            var markedEntry = logEntries.Single(e => e.Level == AxeLogLevel.Info);
            VerifyLogEntry(markedEntry, logEntryData);

            var notMarkedEntry = logEntries.Single(e => e.Level == AxeLogLevel.Error);
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
            
            Assert.Equal(AxeLogLevel.Error, retrivedLogEntry.Level);
            Assert.Equal(exception, retrivedLogEntry.Data);
        }

        [Fact]
        public void should_only_get_log_entries_marked_and_without_default_error_log_entry_given_exception_with_aggregate_exceptions_and_any_ancestor_exception_already_marked()
        {
            object logEntryData = CreateLogEntryData();
            var notMarkedExceptionOne = new Exception("inner exception one");
            var notMarkedExceptionTwo = new Exception("inner exception two");
            var exception = new Exception("", new AggregateException(
                    "aggregate exceptions",
                    notMarkedExceptionOne,
                    notMarkedExceptionTwo))
                .Mark(AxeLogLevel.Info, logEntryData);

            LogEntry retrived = exception.GetLogEntry().Single();

            VerifyLogEntry(retrived, logEntryData);
        }

        [Fact]
        public void should_get_error_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_within_maxlevel()
        {
            object logEntryData = CreateLogEntryData();
            var exception = new Exception(
                "exception level 1", 
                new Exception("exception level 2").Mark(AxeLogLevel.Info, logEntryData));

            LogEntry retrived = exception.GetLogEntry(1).Single();

            Assert.Equal(AxeLogLevel.Error, retrived.Level);
            Assert.Equal(exception, retrived.Data);
        }

        [Fact]
        public void should_get_log_entries_with_exceptions_marked_in_the_maxlevel()
        {
            object logEntryData = CreateLogEntryData();

            var aggregateException = new Exception(
                "level 1", 
                new Exception(
                    "level 2",
                    new Exception(
                        "level 3",
                        new Exception("level 4").Mark(AxeLogLevel.Info, logEntryData))));

            LogEntry retrived = aggregateException.GetLogEntry(4).Single();
            
            VerifyLogEntry(retrived, logEntryData);
        }

        [Fact]
        public void should_get_log_entries_for_exception_contains_aggregate_exception()
        {
            const AxeLogLevel axeLogLevel41 = AxeLogLevel.Info;
            object logEntry41Data = CreateLogEntryData("logEntry 41 Data");

            const AxeLogLevel axeLogLevel52 = AxeLogLevel.Warn;
            object logEntry52Data = CreateLogEntryData("logEntry 52 Data");

            const AxeLogLevel axeLogLevel2 = AxeLogLevel.Error;
            object logEntry2Data = CreateLogEntryData("logEntry 2 Data");

            var exception = new Exception(
                "level 1",
                new Exception("level 2",
                        new AggregateException("level 3",
                            new Exception(
                                "level 4.1",
                                new Exception("level 5.1")).Mark(axeLogLevel41, logEntry41Data),
                            new Exception(
                                "level 4.2",
                                new Exception("level 5.2").Mark(axeLogLevel52, logEntry52Data))))
                    .Mark(axeLogLevel2, logEntry2Data));

            LogEntry[] logEntries = exception.GetLogEntry(6);
            var logEntry41Result = logEntries.Single(e => e.Level == axeLogLevel41);
            var logEntry52Result = logEntries.Single(e => e.Level == axeLogLevel52);
            var logEntry2Result = logEntries.Single(e => e.Level == axeLogLevel2);

            Assert.Equal(3, logEntries.Length);

            VerifyLogEntry(logEntry2Result, logEntry2Data, axeLogLevel2);
            VerifyLogEntry(logEntry41Result, logEntry41Data, axeLogLevel41);
            VerifyLogEntry(logEntry52Result, logEntry52Data, axeLogLevel52);

            Assert.True(logEntries.Select(e => e.AggregateId).Distinct().Count() == 1);
        }

        static void VerifyLogEntry(LogEntry logEntryResult, object logEntryData, AxeLogLevel axeLogLevel = AxeLogLevel.Info)
        {
            Assert.Equal(DateTime.UtcNow.ToString(), logEntryResult.Time.ToString());
            Assert.Equal(logEntryData, logEntryResult.Data);
            Assert.Equal(axeLogLevel, logEntryResult.Level);
        }

        static object CreateLogEntryData(string data = "data name")
        {
            return new { Name = data };
        }
    }
}
