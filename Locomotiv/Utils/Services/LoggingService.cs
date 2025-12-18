using Locomotiv.Utils.Services.Interfaces;
using NLog;

namespace Locomotiv.Utils.Services
{
    public class LoggingService : ILoggingService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void LogInfo(string message)
        {
            Logger.Info(message);
        }

        public void LogWarning(string message)
        {
            Logger.Warn(message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            if (exception != null)
            {
                Logger.Error(exception, message);
            }
            else
            {
                Logger.Error(message);
            }
        }

        public void LogDebug(string message)
        {
            Logger.Debug(message);
        }
    }
}
