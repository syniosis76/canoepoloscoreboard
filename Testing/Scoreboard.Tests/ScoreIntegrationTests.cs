using FluentAssertions;
using Scoreboard.Tests.TestDoubles;

namespace Scoreboard.Tests;

[Collection("Score Tests")]
public class ScoreIntegrationTests : IDisposable
{
    private readonly FakeTimeService _time;
    private readonly Score _score;
    private readonly Game _game;

    public ScoreIntegrationTests()
    {
        _time = new FakeTimeService(new DateTime(2026, 7, 10, 8, 0, 0));
        _score = new Score(_time);

        _game = new Game { Team1 = "Team A", Team2 = "Team B" };
        _game.Periods.Add(new GamePeriod
        {
            Name = "Period 1",
            StartTime = _time.Now,
            EndTime = _time.Now.AddMinutes(10)
        });
        _game.Periods.Add(new GamePeriod
        {
            Name = "Period 2",
            StartTime = _time.Now.AddMinutes(12),
            EndTime = _time.Now.AddMinutes(22)
        });
        _game.Periods.SetFirstPeriod();

        _score.Games.Add(_game);
        _score.StartTimer();
    }

    public void Dispose()
    {
        _score.StopServer();
    }

    [Fact]
    public void StartGames_SelectsCurrentGame()
    {
        _score.StartGames();

        _score.CurrentGame.Should().BeSameAs(_game);
    }

    [Fact]
    public void TimerTick_WhenNotPaused_SetsPeriodActive()
    {
        _score.StartGames();

        _time.AdvanceSeconds(1);

        _game.Periods.CurrentPeriod!.Status.Should().Be(GamePeriodStatus.Active);
    }

    [Fact]
    public void Team1Goal_IncrementsScore()
    {
        _score.StartGames();
        var initialScore = _game.Team1Score;

        _score.Team1Goal("Player 1");

        _game.Team1Score.Should().Be(initialScore + 1);
    }

    [Fact]
    public void Team2Goal_IncrementsScore()
    {
        _score.StartGames();
        var initialScore = _game.Team2Score;

        _score.Team2Goal("Player 1");

        _game.Team2Score.Should().Be(initialScore + 1);
    }

    [Fact]
    public void Pause_SetsPausedTrue()
    {
        _score.StartGames();

        _score.Pause();

        _score.Paused.Should().BeTrue();
    }

    [Fact]
    public void Resume_AfterPause_ClearsPaused()
    {
        _score.StartGames();
        _score.Pause();

        _score.Resume();

        _score.Paused.Should().BeFalse();
    }

    [Fact]
    public void TimerTick_WhenPaused_DoesNotAdvanceTime()
    {
        _score.StartGames();
        _score.Pause();
        var timeBefore = _game.Periods.CurrentPeriod!.TimeRemaining;

        _time.AdvanceSeconds(5);

        var timeAfter = _game.Periods.CurrentPeriod!.TimeRemaining;
        timeAfter.Should().Be(timeBefore);
    }

    [Fact]
    public void ShotClock_Reset_SetsToTimeLimit()
    {
        _score.StartGames();
        _time.AdvanceSeconds(1);
        _score.ShotClockTimeLimit = 60;
        _score.StopShotTime();
        _score.ShotTime = 10;

        _score.ResetShotTime();

        _score.ShotTime.Should().Be(60);
    }

    [Fact]
    public void ShotClock_Decrement_ReducesTime()
    {
        _score.StartGames();
        _time.AdvanceSeconds(1);
        _score.ShotClockTimeLimit = 60;
        _score.ResetShotTime();
        var initialShotTime = _score.ShotTime;

        _score.DecrementShotTime();

        _score.ShotTime.Should().Be(initialShotTime - 1);
    }

    [Fact]
    public void ShotClock_Increment_IncreasesTime()
    {
        _score.StartGames();
        _time.AdvanceSeconds(1);
        _score.ShotClockTimeLimit = 60;
        _score.ResetShotTime();
        _score.ShotTime = 10;

        _score.IncrementShotTime();

        _score.ShotTime.Should().Be(11);
    }

    [Fact]
    public void AddExtraPeriod_CreatesOvertime()
    {
        _score.StartGames();

        _score.AddExtraPeriod(_game);

        _game.Periods.Should().HaveCount(3);
        _game.Periods[2].IsExtraPeriod.Should().BeTrue();
    }

    [Fact]
    public void NextGame_WhenNoMoreGames_CurrentGameStays()
    {
        _score.StartGames();
        _score.CurrentGame = _game;

        _score.NextGame(false);

        _score.CurrentGame.Should().BeNull();
    }

    [Fact]
    public void NextGame_WhenMultipleGames_Advances()
    {
        var game2 = new Game { Team1 = "Team C", Team2 = "Team D" };
        game2.Periods.Add(new GamePeriod
        {
            Name = "Period 1",
            StartTime = _time.Now.AddMinutes(25),
            EndTime = _time.Now.AddMinutes(35)
        });
        game2.Periods.SetFirstPeriod();
        _score.Games.Add(game2);
        _score.StartGames();
        _score.CurrentGame = _game;

        _score.NextGame(false);

        _score.CurrentGame.Should().Be(game2);
    }
}
