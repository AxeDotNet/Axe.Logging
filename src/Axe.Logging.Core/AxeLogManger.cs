using System.Diagnostics;

namespace Axe.Logging.Core
{
    public class AxeLogManger
    {
        public static IAxeLogger GetLogger(string name) { return GetLogger(name, AxeLogSetting.Default); }

        public static IAxeLogger GetLogger(string name, AxeLogSetting axeLogSetting)
        {
            var logSetting = axeLogSetting ?? AxeLogSetting.Default;
            switch (logSetting.LoggingBackend)
            {
                case "NLog":
                    return new NLogLogger(name);
                default:
                    return new DummyLogger(name);
            }
        }

        public static IAxeLogger GetCurrentClassLogger()
        {
            return GetCurrentClassLogger(new StackFrame(1, false).GetMethod().DeclaringType.FullName, AxeLogSetting.Default);
        }

        public static IAxeLogger GetCurrentClassLogger(AxeLogSetting axeLogSetting)
        {
            return GetCurrentClassLogger(new StackFrame(1, false).GetMethod().DeclaringType.FullName, axeLogSetting);
        }

        private static IAxeLogger GetCurrentClassLogger(string className, AxeLogSetting axeLogSetting)
        {
            var logSetting = axeLogSetting ?? AxeLogSetting.Default;
            return GetLogger(className, logSetting);
        }

    }
}