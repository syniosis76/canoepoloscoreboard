using System;

namespace Utilities
{
    public interface ITimeService
    {
        DateTime Now { get; }
        bool IsRunning { get; }
        event EventHandler Tick;
        void Start();
        void Stop();
    }
}
