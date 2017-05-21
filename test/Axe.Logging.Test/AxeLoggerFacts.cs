using System;
using System.Collections.Generic;
using System.Linq;
using Axe.Logging.Core;
using Xunit;

namespace Axe.Logging.Test
{
    public class AxeLoggerFacts
    {
        [Fact]
        public void should_log_for_general_log()
        {
            FakeAxeLogger fakeAxeLogger = new FakeAxeLogger();
            var data = new { Data = "data" };
            fakeAxeLogger.Log(AxeLogLevel.Info, data);

            var logEntry = fakeAxeLogger.Logs.Single();

            Assert.Equal(AxeLogLevel.Info, logEntry.Level);
            Assert.Equal(DateTime.UtcNow.Date, logEntry.Time.Date);
            Assert.Equal(data, logEntry.Data);
        }

        [Fact]
        public void should_log_for_exception()
        {
            FakeAxeLogger fakeAxeLogger = new FakeAxeLogger();

            var dataOnParrent = new { Name = "logEntryOnParent" };
            var timeOnParrent = DateTime.UtcNow.AddMinutes(-1);
            var logEntryOnParent = new LogEntryMark(timeOnParrent, dataOnParrent, AxeLogLevel.Info );
            var dataOnInner = new { Name = "logEntryOnInner" };
            var timeOnInner = DateTime.UtcNow;
            var logEntryOnInner = new LogEntryMark(timeOnInner, dataOnInner, AxeLogLevel.Warn);
            var innerException = new Exception("inner").Mark(logEntryOnInner.Level, logEntryOnInner.Data);
            var parentException = new Exception("parent", innerException).Mark(logEntryOnParent.Level, logEntryOnParent.Data);

            fakeAxeLogger.Log(parentException);

            Assert.Equal(2, fakeAxeLogger.Logs.Count);

            Assert.Equal(AxeLogLevel.Info, fakeAxeLogger.Logs[0].Level);
            Assert.Equal(timeOnParrent.Date, fakeAxeLogger.Logs[0].Time.Date);
            Assert.Equal(dataOnParrent, fakeAxeLogger.Logs[0].Data);

            Assert.Equal(AxeLogLevel.Warn, fakeAxeLogger.Logs[1].Level);
            Assert.Equal(timeOnInner.Date, fakeAxeLogger.Logs[1].Time.Date);
            Assert.Equal(dataOnInner, fakeAxeLogger.Logs[1].Data);
        }

        [Fact]
        public void should_log_as_info()
        {
            FakeAxeLogger fakeAxeLogger = new FakeAxeLogger();
            var data = new { Data = "data" };
            fakeAxeLogger.Info(data);

            var logEntry = fakeAxeLogger.Logs.Single();

            Assert.Equal(AxeLogLevel.Info, logEntry.Level);
            Assert.Equal(data, logEntry.Data);
        }

        [Fact]
        public void should_log_as_warn()
        {
            FakeAxeLogger fakeAxeLogger = new FakeAxeLogger();
            var data = new { Data = "data" };
            fakeAxeLogger.Warn(data);

            var logEntry = fakeAxeLogger.Logs.Single();

            Assert.Equal(AxeLogLevel.Warn, logEntry.Level);
            Assert.Equal(data, logEntry.Data);
        }

        [Fact]
        public void should_log_as_error()
        {
            FakeAxeLogger fakeAxeLogger = new FakeAxeLogger();
            var data = new { Data = "data" };
            fakeAxeLogger.Error(data);

            var logEntry = fakeAxeLogger.Logs.Single();

            Assert.Equal(AxeLogLevel.Error, logEntry.Level);
            Assert.Equal(data, logEntry.Data);
        }
    }

    public class FakeAxeLogger : LoggerBase
    {
        public List<LogEntry> Logs { get; } = new List<LogEntry>();

        public override void RecordLogEntry(LogEntry logEntry)
        {
            Logs.Add(logEntry);      
        }
    }
}