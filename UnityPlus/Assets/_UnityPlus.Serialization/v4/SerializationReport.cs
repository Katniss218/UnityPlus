
using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Fatal
    }

    public struct LogEntry
    {
        public LogLevel Level;
        public string Message;
        public string ObjectPath;
        public object ContextObject;
        public SerializedData ContextNode;

        public override string ToString() => $"[{Level}] {ObjectPath}: {Message}";
    }

    public class SerializationReport
    {
        private readonly List<LogEntry> _logs = new List<LogEntry>();
        public bool HasFatalErrors { get; private set; }

        public IReadOnlyList<LogEntry> Logs => _logs;

        public void Log( LogLevel level, string message, SerializationState state = null, object target = null )
        {
            if( level == LogLevel.Fatal ) HasFatalErrors = true;

            string path = "Unknown";
            if( state != null )
            {
                path = PathBuilder.BuildPath( state.Stack );
            }

            var entry = new LogEntry
            {
                Level = level,
                Message = message,
                ObjectPath = path,
                ContextObject = target
            };

            _logs.Add( entry );

            // Mirror to Unity Debug for immediate visibility
            if( level == LogLevel.Error || level == LogLevel.Fatal )
                UnityEngine.Debug.LogError( entry.ToString() );
            else if( level == LogLevel.Warning )
                UnityEngine.Debug.LogWarning( entry.ToString() );
        }

        public void Clear()
        {
            _logs.Clear();
            HasFatalErrors = false;
        }
    }
}
