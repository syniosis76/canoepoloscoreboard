using FluentAssertions;

namespace Scoreboard.Tests;

public class GamePeriodTests
{
    [Fact]
    public void TimeRemaining_WhenActive_ReturnsEndTimeMinusNow()
    {
        var now = DateTime.Now;
        var period = new GamePeriod
        {
            StartTime = now.AddMinutes(-5),
            EndTime = now.AddMinutes(5)
        };

        var remaining = period.TimeRemaining;

        remaining.Should().BeCloseTo(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TimeRemaining_WhenEnded_ReturnsZero()
    {
        var now = DateTime.Now;
        var period = new GamePeriod
        {
            StartTime = now.AddMinutes(-10),
            EndTime = now.AddMinutes(-5)
        };
        period.UpdatePeriod();

        var remaining = period.TimeRemaining;

        remaining.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void UpdatePeriod_WhenBeforeStart_ReturnsPending()
    {
        var now = DateTime.Now;
        var period = new GamePeriod
        {
            StartTime = now.AddMinutes(5),
            EndTime = now.AddMinutes(10)
        };

        var status = period.UpdatePeriod();

        status.Should().Be(GamePeriodStatus.Pending);
    }

    [Fact]
    public void UpdatePeriod_WhenAfterStartBeforeEnd_ReturnsActive()
    {
        var now = DateTime.Now;
        var period = new GamePeriod
        {
            StartTime = now.AddMinutes(-5),
            EndTime = now.AddMinutes(5)
        };

        var status = period.UpdatePeriod();

        status.Should().Be(GamePeriodStatus.Active);
    }

    [Fact]
    public void UpdatePeriod_WhenAfterEnd_ReturnsEnded()
    {
        var now = DateTime.Now;
        var period = new GamePeriod
        {
            StartTime = now.AddMinutes(-10),
            EndTime = now.AddMinutes(-5)
        };

        var status = period.UpdatePeriod();

        status.Should().Be(GamePeriodStatus.Ended);
    }

    [Fact]
    public void EndNow_SetsEndTimeToNow()
    {
        var before = DateTime.Now;
        var period = new GamePeriod
        {
            StartTime = before.AddMinutes(-10),
            EndTime = before.AddMinutes(10)
        };

        period.EndNow();

        period.EndTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        period.Status.Should().Be(GamePeriodStatus.Ended);
    }

    [Fact]
    public void ModifyGameTime_WhenActive_AdjustsEndTime()
    {
        var now = DateTime.Now;
        var period = new GamePeriod
        {
            StartTime = now.AddMinutes(-5),
            EndTime = now.AddMinutes(5)
        };
        var originalEnd = period.EndTime;

        period.ModifyGameTime(30, false);

        period.EndTime.Should().BeCloseTo(originalEnd.AddSeconds(30), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Description_WhenActive_ReturnsPeriodName()
    {
        var period = new GamePeriod
        {
            Name = "Period 1",
            StartTime = DateTime.Now.AddMinutes(-5),
            EndTime = DateTime.Now.AddMinutes(5)
        };

        period.Description.Should().Be("Period 1");
    }

    [Fact]
    public void Description_WhenEnded_ContainsEndedText()
    {
        var period = new GamePeriod
        {
            Name = "Period 1",
            StartTime = DateTime.Now.AddMinutes(-10),
            EndTime = DateTime.Now.AddMinutes(-1)
        };
        period.UpdatePeriod();

        period.Description.Should().Contain("Ended");
    }

    [Fact]
    public void IsExtraPeriod_DefaultsToFalse()
    {
        var period = new GamePeriod();

        period.IsExtraPeriod.Should().BeFalse();
    }

    [Fact]
    public void Status_WhenPending_ReturnsPending()
    {
        var period = new GamePeriod
        {
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(2)
        };

        period.Status.Should().Be(GamePeriodStatus.Pending);
    }
}
