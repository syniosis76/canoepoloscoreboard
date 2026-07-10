using FluentAssertions;

namespace Scoreboard.Tests;

public class GamePeriodListTests
{
    [Fact]
    public void CurrentPeriod_WhenEmpty_ReturnsNull()
    {
        var list = new GamePeriodList();

        list.CurrentPeriod.Should().BeNull();
    }

    [Fact]
    public void SetFirstPeriod_SetsCurrentToFirst()
    {
        var list = new GamePeriodList
        {
            new GamePeriod { Name = "Period 1" },
            new GamePeriod { Name = "Period 2" }
        };

        list.SetFirstPeriod();

        list.CurrentPeriod.Should().NotBeNull();
        list.CurrentPeriod!.Name.Should().Be("Period 1");
    }

    [Fact]
    public void NextPeriod_AdvancesToNext()
    {
        var list = new GamePeriodList
        {
            new GamePeriod { Name = "Period 1" },
            new GamePeriod { Name = "Period 2" }
        };
        list.SetFirstPeriod();

        list.NextPeriod();

        list.CurrentPeriod!.Name.Should().Be("Period 2");
    }

    [Fact]
    public void NextPeriod_WhenAtEnd_ReturnsNullAndCurrentNull()
    {
        var list = new GamePeriodList
        {
            new GamePeriod { Name = "Period 1" }
        };
        list.SetFirstPeriod();

        var result = list.NextPeriod();

        result.Should().BeNull();
        list.CurrentPeriod.Should().BeNull();
    }

    [Fact]
    public void PreviousPeriod_GoesBack()
    {
        var list = new GamePeriodList
        {
            new GamePeriod { Name = "Period 1" },
            new GamePeriod { Name = "Period 2" }
        };
        list.SetFirstPeriod();
        list.NextPeriod();

        list.PreviousPeriod();

        list.CurrentPeriod!.Name.Should().Be("Period 1");
    }

    [Fact]
    public void HasEnded_WhenAllPeriodsEnded_ReturnsTrue()
    {
        var now = DateTime.Now;
        var list = new GamePeriodList
        {
            new GamePeriod { StartTime = now.AddMinutes(-10), EndTime = now.AddMinutes(-5) },
            new GamePeriod { StartTime = now.AddMinutes(-4), EndTime = now.AddMinutes(-1) }
        };
        list.SetFirstPeriod();
        list.NextPeriod();

        var hasEnded = list.HasEnded;

        hasEnded.Should().BeTrue();
    }

    [Fact]
    public void HasEnded_WhenEmpty_ReturnsFalse()
    {
        var list = new GamePeriodList();

        list.HasEnded.Should().BeFalse();
    }

    [Fact]
    public void ModifyFollowingTimes_ShiftsRemainingPeriods()
    {
        var now = DateTime.Now;
        var list = new GamePeriodList
        {
            new GamePeriod
            {
                Name = "Period 1",
                StartTime = now.AddMinutes(-10),
                EndTime = now.AddMinutes(5)
            },
            new GamePeriod
            {
                Name = "Period 2",
                StartTime = now.AddMinutes(7),
                EndTime = now.AddMinutes(17)
            }
        };

        list.ModifyFollowingTimes(list[0], TimeSpan.FromMinutes(2), false);

        list[1].StartTime.Should().Be(now.AddMinutes(9));
        list[1].EndTime.Should().Be(now.AddMinutes(19));
    }

    [Fact]
    public void ModifyFollowingTimes_StartingAtIndex_ShiftsFromThatIndex()
    {
        var now = DateTime.Now;
        var list = new GamePeriodList
        {
            new GamePeriod
            {
                Name = "Period 1",
                StartTime = now.AddMinutes(-10),
                EndTime = now.AddMinutes(5)
            },
            new GamePeriod
            {
                Name = "Period 2",
                StartTime = now.AddMinutes(7),
                EndTime = now.AddMinutes(17)
            },
            new GamePeriod
            {
                Name = "Period 3",
                StartTime = now.AddMinutes(19),
                EndTime = now.AddMinutes(29)
            }
        };

        list.ModifyFollowingTimes(list[1], TimeSpan.FromMinutes(3), false);

        list[1].StartTime.Should().Be(now.AddMinutes(7));
        list[1].EndTime.Should().Be(now.AddMinutes(17));
        list[2].StartTime.Should().Be(now.AddMinutes(22));
        list[2].EndTime.Should().Be(now.AddMinutes(32));
    }
}
