using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Library.Extensions
{
    public static class ObjectExtensions
    {
        public static void LogInEditor(this object _, object message, LogType logType = LogType.Log,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string path = "")
        {
#if UNITY_EDITOR
            var logMessage = $"{Path.GetFileNameWithoutExtension(path)}::{caller} => {message}";
            switch (logType)
            {
            case LogType.Log:
                Debug.Log($"<color=green>{logMessage}</color>");
                break;
            case LogType.Warning:
                Debug.Log($"<color=yellow>{logMessage}</color>");
                break;
            case LogType.Assert:
            case LogType.Error:
            case LogType.Exception:
                Debug.Log($"<color=red>{logMessage}</color>");
                break;
            default:
                Debug.Log(logMessage);
                break;
            }
#endif
        }
    }
}