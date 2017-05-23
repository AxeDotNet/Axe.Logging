using System;

namespace Axe.Logging.Core
{
    public interface IAxeLogger
    {
        void Log(AxeLogLevel level, object data);

        void Log(Exception exception);

        void Info(object data);

        void Error(object data);

        void Warn(object data);
    }
}