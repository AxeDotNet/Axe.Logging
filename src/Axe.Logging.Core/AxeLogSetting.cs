using System;
using Axe.Logging.Core.NullValues;

namespace Axe.Logging.Core
{
    public class AxeLogSetting
    {
        int maxExceptionRecursionLevel = 10;

        public static AxeLogSetting Default { get;} = new AxeLogSetting();

        public ILoggingBackend LoggingBackend { get; set; } = new DummyLoggingBackend();

        public int MaxExceptionRecursionLevel
        {
            get => maxExceptionRecursionLevel;
            set
            {
                if (value <= 0 && value > 10)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                maxExceptionRecursionLevel = value;
            }
        }
    }
}