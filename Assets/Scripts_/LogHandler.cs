using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    void Awake()
    {

        // Redirect Unity's log output to our custom handler
        Application.logMessageReceived += HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Format the log message
        string logEntry = $"{System.DateTime.Now}: [{type}] {logString}\n";
        if (type == LogType.Error || type == LogType.Exception)
        {
            logEntry += $"Stack Trace: {stackTrace}\n";
        }

        // Write the log message to the log file
        UtilityFunc.AppendToLog(logEntry);
    }

    void OnDestroy()
    {
        // Unregister the log handler when the object is destroyed
        Application.logMessageReceived -= HandleLog;
    }
}
