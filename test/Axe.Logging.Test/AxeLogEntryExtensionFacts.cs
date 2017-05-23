using System;
using System.Linq;
using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.Test
{
    public class AxeLogEntryExtensionFacts
    {
        [Fact]
        public void should_get_info_log_entry_from_exception_marked_as_info()
        {
            object logEntryData = CreateLogEntryData();
            Exception exception = new Exception().MarkAsInfo(logEntryData);

            LogEntry logEntryResult = exception.GetLogEntries().Single();

            VerifyLogEntry(logEntryResult, logEntryData, AxeLogLevel.Info);
        }

        [Fact]
        public void should_get_warn_log_entry_from_exception_marked_as_warn()
        {
            object logEntryData = CreateLogEntryData();
            Exception exception = new Exception().MarkAsWarn(logEntryData);

            LogEntry logEntryResult = exception.GetLogEntries().Single();

            VerifyLogEntry(logEntryResult, logEntryData, AxeLogLevel.Warn);
        }

        [Fact]
        public void should_get_error_log_entry_from_exception_marked_as_error()
        {
            object logEntryData = CreateLogEntryData();
            Exception exception = new Exception().MarkAsError(logEntryData);

            LogEntry logEntryResult = exception.GetLogEntries().Single();

            VerifyLogEntry(logEntryResult, logEntryData, AxeLogLevel.Error);
        }

        [Fact]
        public void should_throw_argumet_null_exception_when_mark_exception_given_exception_is_null()
        {
            Exception exception = null;
            object logEntryData = CreateLogEntryData();

            Assert.Throws<ArgumentNullException>(() => exception.MarkAsInfo(logEntryData));
        }

        [Fact]
        public void should_replace_the_marked_log_entry_when_mark_a_exception_already_marked()
        {
            object oldLogEntryData = CreateLogEntryData("oldLogEntryData");
            object newLogEntryData = CreateLogEntryData("newLogEntryData");

            Exception exception = new Exception().MarkAsInfo(oldLogEntryData);
            exception.MarkAsError(newLogEntryData);

            VerifyLogEntry(exception.GetLogEntries().Single(), newLogEntryData, AxeLogLevel.Error);
        }

        [Fact]
        public void should_get_empty_log_entry_collection_when_exception_is_null()
        {
            Exception exception = null;

            Assert.Empty(exception.GetLogEntries());
        }

        [Fact]
        public void should_get_error_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_with_log_entry()
        {
            var exception = new Exception();

            LogEntry logEntry = exception.GetLogEntries().Single();

            Assert.Equal(DateTime.UtcNow, logEntry.Time);
            Assert.Equal(exception, logEntry.Data);
            Assert.Equal(AxeLogLevel.Error, logEntry.Level);
        }

        [Fact]
        public void should_get_log_entry_of_exception_if_log_entry_existed_in_inner_exception()
        {
            object logEntryData = CreateLogEntryData();
            Exception innerExceptionWithLogEntry = new Exception().MarkAsInfo(logEntryData);
            var exception = new Exception("parent", innerExceptionWithLogEntry);

            LogEntry logEntryResult = exception.GetLogEntries().Single();

            VerifyLogEntry(logEntryResult, logEntryData, AxeLogLevel.Info);
        }

        [Fact]
        public void should_get_all_log_entries_with_same_id_for_multiple_marked_exceptions()
        {
            object logEntryOnParentData = CreateLogEntryData("logEntryOnParentData");
            object logEntryOnInnerData = CreateLogEntryData("logEntryOnInnerData");

            Exception innerException = new Exception("inner").MarkAsInfo(logEntryOnInnerData);
            Exception parentException = new Exception("parent", innerException).MarkAsInfo(logEntryOnParentData);

            LogEntry[] logEntries = parentException.GetLogEntries();

            Assert.Equal(2, logEntries.Length);

            VerifyLogEntry(logEntries[0], logEntryOnParentData, AxeLogLevel.Info);
            VerifyLogEntry(logEntries[1], logEntryOnInnerData, AxeLogLevel.Info);

            Assert.True(logEntries[0].AggregateId == logEntries[1].AggregateId);
        }

        [Fact]
        public void should_get_all_log_entries_with_same_id_for_multiple_marked_aggregated_exceptions()
        {
            object logEntryData1 = CreateLogEntryData("logEntry 1 Data");
            object logEntryData2 = CreateLogEntryData("logEntry 2 Data");

            Exception innerExceptionMarkedOne = new Exception("inner exception one").MarkAsInfo(logEntryData1);
            Exception innerExceptionMarkedTwo = new Exception("inner exception two").MarkAsWarn(logEntryData2);
            var exception = new AggregateException(
                "aggregate exceptions", 
                innerExceptionMarkedOne, 
                innerExceptionMarkedTwo);

            LogEntry[] logEntries = exception.GetLogEntries();
            var innerExceptionMarkedOneResult = logEntries.Single(l => l.Level == AxeLogLevel.Info);
            var innerExceptionMarkedTwoResult = logEntries.Single(l => l.Level == AxeLogLevel.Warn);

            Assert.Equal(2, logEntries.Length);

            VerifyLogEntry(innerExceptionMarkedOneResult, logEntryData1, AxeLogLevel.Info);
            VerifyLogEntry(innerExceptionMarkedTwoResult, logEntryData2, AxeLogLevel.Warn);

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

            LogEntry logEntry = exception.GetLogEntries().Single();

            Assert.Equal(AxeLogLevel.Error, logEntry.Level);
            Assert.Equal(exception, logEntry.Data);
        }

        [Fact]
        public void should_get_error_log_entries_when_not_all_branches_marked()
        {
            object logEntryData = CreateLogEntryData();
            var notMarkedException = new Exception();
            var markedException = new Exception().MarkAsInfo(logEntryData);
            var exception = new AggregateException("aggregate exceptions", notMarkedException, markedException);

            LogEntry[] logEntries = exception.GetLogEntries();

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

            LogEntry[] logEntries = exception.GetLogEntries();

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
                .MarkAsInfo(logEntryData);

            LogEntry retrived = exception.GetLogEntries().Single();

            VerifyLogEntry(retrived, logEntryData);
        }

        [Fact]
        public void should_get_error_level_log_entry_when_get_log_entry_from_exception_given_exception_not_marked_within_maxlevel()
        {
            object logEntryData = CreateLogEntryData();
            var exception = new Exception(
                "exception level 1", 
                new Exception("exception level 2").MarkAsInfo(logEntryData));

            LogEntry retrived = exception.GetLogEntries(1).Single();

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
                        new Exception("level 4").MarkAsInfo(logEntryData))));

            LogEntry retrived = aggregateException.GetLogEntries(4).Single();
            
            VerifyLogEntry(retrived, logEntryData);
        }

        [Fact]
        public void should_get_log_entries_for_exception_contains_aggregate_exception()
        {
            object logEntry41Data = CreateLogEntryData("logEntry 41 Data");
            object logEntry52Data = CreateLogEntryData("logEntry 52 Data");
            object logEntry2Data = CreateLogEntryData("logEntry 2 Data");

            var exception = new Exception(
                "level 1",
                new Exception("level 2",
                        new AggregateException("level 3",
                            new Exception(
                                "level 4.1",
                                new Exception("level 5.1")).MarkAsInfo(logEntry41Data),
                            new Exception(
                                "level 4.2",
                                new Exception("level 5.2").MarkAsWarn(logEntry52Data))))
                    .MarkAsError(logEntry2Data));

            LogEntry[] logEntries = exception.GetLogEntries(6);
            var logEntry41Result = logEntries.Single(e => e.Level == AxeLogLevel.Info);
            var logEntry52Result = logEntries.Single(e => e.Level == AxeLogLevel.Warn);
            var logEntry2Result = logEntries.Single(e => e.Level == AxeLogLevel.Error);

            Assert.Equal(3, logEntries.Length);

            VerifyLogEntry(logEntry2Result, logEntry2Data, AxeLogLevel.Error);
            VerifyLogEntry(logEntry41Result, logEntry41Data, AxeLogLevel.Info);
            VerifyLogEntry(logEntry52Result, logEntry52Data, AxeLogLevel.Warn);

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
