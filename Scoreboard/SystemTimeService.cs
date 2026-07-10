using System;
using System.Windows.Threading;
using Utilities;

namespace Scoreboard
{
    public class SystemTimeService : ITimeService
    {
        private readonly DispatcherTimer _timer = new();

        public DateTime Now => DateTime.Now;

        public bool IsRunning => _timer.IsEnabled;

        public event EventHandler Tick
        {
            add => _timer.Tick += value;
            remove => _timer.Tick -= value;
        }

        public void Start()
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.IsEnabled = true;
        }

        public void Stop()
        {
            _timer.IsEnabled = false;
        }
    }
}
