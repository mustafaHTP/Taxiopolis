using UnityEngine;

namespace Game.Utils
{
    public static class LogX
    {
        public static void LogInfo(string message, UnityEngine.Object context = null)
        {
            Log(message, LogLevel.INFO, "cyan", context);
        }

        public static void LogWarning(string message, UnityEngine.Object context = null)
        {
            Log(message, LogLevel.WARNING, "yellow", context);
        }

        public static void LogError(string message, UnityEngine.Object context = null)
        {
            Log(message, LogLevel.ERROR, "red", context);
        }

        public static void LogDebug(string message, UnityEngine.Object context = null)
        {
            Log(message, LogLevel.DEBUG, "white", context);
        }

        private static void Log(
            string message,
            LogLevel logLevel,
            string color,
            UnityEngine.Object context = null)
        {
            string contextName = context != null ? $"[{context.name}] " : "";

            switch (logLevel)
            {
                case LogLevel.DEBUG:
                    UnityEngine.Debug.Log($"{contextName} <color={color}>[{logLevel}] {message}</color>");
                    break;
                case LogLevel.INFO:
                    UnityEngine.Debug.Log($"{contextName} <color={color}>[{logLevel}] {message}</color>");
                    break;
                case LogLevel.WARNING:
                    UnityEngine.Debug.LogWarning($"{contextName} <color={color}>[{logLevel}] {message}</color>");
                    break;
                case LogLevel.ERROR:
                    UnityEngine.Debug.LogError($"{contextName} <color={color}>[{logLevel}] {message}</color>");
                    break;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void UpdateLogActivation()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.unityLogger.logEnabled = true;
#else
            UnityEngine.Debug.unityLogger.logEnabled = false;
#endif
        }
    }

    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR
    }
}
