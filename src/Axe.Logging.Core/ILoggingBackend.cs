namespace Axe.Logging.Core
{
    public interface ILoggingBackend
    {
        IAxeLogger GetLogger(string name);
    }
}