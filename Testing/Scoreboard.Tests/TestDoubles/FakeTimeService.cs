using Utilities;

namespace Scoreboard.Tests.TestDoubles;

public class FakeTimeService : ITimeService
{
    private DateTime _now;

    public FakeTimeService() : this(new DateTime(2026, 7, 10, 8, 0, 0))
    {
    }

    public FakeTimeService(DateTime startTime)
    {
        _now = startTime;
    }

    public DateTime Now => _now;
    public bool IsRunning { get; private set; }

    public event EventHandler? Tick;

    public void Start()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Advance(TimeSpan span)
    {
        _now = _now.Add(span);
    }

    public void AdvanceSeconds(int seconds)
    {
        Advance(TimeSpan.FromSeconds(seconds));
        Tick?.Invoke(this, EventArgs.Empty);
    }

    public void TickOnce()
    {
        Tick?.Invoke(this, EventArgs.Empty);
    }
}
