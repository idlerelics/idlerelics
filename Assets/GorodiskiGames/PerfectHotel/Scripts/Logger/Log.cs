using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Simple logging wrapper around Unity's Debug.Log system.
    /// Provides a single place to enable/disable all game logs.
    ///
    /// 'static' class means all methods can be called without creating an instance:
    ///   Log.Info("Something happened");
    ///   Log.Error("Something went wrong");
    ///
    /// Unity's Debug class outputs to:
    /// - The Console window in the Unity Editor
    /// - The device log on mobile (viewable via Xcode/Android Logcat)
    /// </summary>
    public static class Log
    {
        /// <summary>Set to false to suppress all Info logs (errors/warnings still show).</summary>
        public static bool IsLogsEnabled = true;

        /// <summary>
        /// Logs an informational message. Respects the IsLogsEnabled flag.
        /// Shows as white text in Unity's Console.
        /// </summary>
        public static void Info(object message)
        {
            if (!IsLogsEnabled)
                return;

            Debug.Log(message);
        }

        /// <summary>
        /// Logs an exception with its full stack trace. Always shows (not affected by IsLogsEnabled).
        /// Shows as red text in Unity's Console.
        /// </summary>
        public static void Exception(Exception exception)
        {
            Debug.LogException(exception);
        }

        /// <summary>
        /// Logs a warning message. Always shows. Yellow text in Unity's Console.
        /// </summary>
        public static void Warning(object message)
        {
            Debug.LogWarning(message);
        }

        /// <summary>
        /// Logs an error message. Always shows. Red text in Unity's Console.
        /// </summary>
        public static void Error(string error)
        {
            Debug.LogError(error);
        }
    }
}
