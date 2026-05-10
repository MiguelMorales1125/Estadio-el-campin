using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StadiumSystem.Services
{
    public enum LogLevel { Debug = 0, Info = 1, Warn = 2, Error = 3 }

    public record TerminalLogEntry(DateTime Timestamp, LogLevel Level, string Message);

    public interface ITerminalLogService
    {
        void Add(string message);
        void Add(LogLevel level, string message);
        IReadOnlyList<TerminalLogEntry> GetAll();
        void Clear();
        LogLevel MinimumLevel { get; set; }
    }

    public sealed class TerminalLogService : ITerminalLogService
    {
        private readonly List<TerminalLogEntry> _entries = new();
        private readonly ReaderWriterLockSlim _lock = new();

        public LogLevel MinimumLevel { get; set; }

        public TerminalLogService(LogLevel minLevel = LogLevel.Info)
        {
            MinimumLevel = minLevel;
        }

        public void Add(string message) => Add(LogLevel.Info, message);

        public void Add(LogLevel level, string message)
        {
            if (level < MinimumLevel) return;
            var entry = new TerminalLogEntry(DateTime.UtcNow, level, message);
            _lock.EnterWriteLock();
            try { _entries.Add(entry); }
            finally { _lock.ExitWriteLock(); }
        }

        public IReadOnlyList<TerminalLogEntry> GetAll()
        {
            _lock.EnterReadLock();
            try { return _entries.ToList().AsReadOnly(); }
            finally { _lock.ExitReadLock(); }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try { _entries.Clear(); }
            finally { _lock.ExitWriteLock(); }
        }
    }
}
