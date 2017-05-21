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

            var dataOnParrentException = new { Name = "logEntryOnParent" };
            var dataOnInnerException = new { Name = "logEntryOnInner" };
            var innerException = new Exception("inner").MarkAsWarn(dataOnInnerException);
            var parentException = new Exception("parent", innerException).MarkAsInfo(dataOnParrentException);

            fakeAxeLogger.Log(parentException);

            var logs = fakeAxeLogger.Logs;
            var logFromParentException = logs.Single(e => e.Level == AxeLogLevel.Info);
            var logFromInnerException = logs.Single(e => e.Level == AxeLogLevel.Warn);

            Assert.Equal(2, logs.Count);

            Assert.Equal(AxeLogLevel.Info, logFromParentException.Level);
            Assert.Equal(DateTime.UtcNow.ToString(), logFromParentException.Time.ToString());
            Assert.Equal(dataOnParrentException, logFromParentException.Data);

            Assert.Equal(AxeLogLevel.Warn, logFromInnerException.Level);
            Assert.Equal(DateTime.UtcNow.ToString(), logFromInnerException.Time.ToString());
            Assert.Equal(dataOnInnerException, logFromInnerException.Data);
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