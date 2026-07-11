using System.Reflection;
using FluentAssertions;
using Scoreboard.Tests.TestDoubles;

namespace Scoreboard.Tests;

[Collection("Score Tests")]

public class HalfTimeSwapAndGameTransitionTests : IDisposable
{
    private readonly FakeTimeService _time;
    private readonly Score _score;

    public HalfTimeSwapAndGameTransitionTests()
    {
        _time = new FakeTimeService(new DateTime(2026, 7, 10, 8, 0, 0));
        _score = new Score(_time);
        _score.StartTimer();
    }

    public void Dispose()
    {
        _score.StopServer();
    }

    private Game CreateGame(params (string name, DateTime start, DateTime end)[] periods)
    {
        var game = new Game();
        foreach (var (name, start, end) in periods)
        {
            game.Periods.Add(new GamePeriod { Name = name, StartTime = start, EndTime = end });
        }
        foreach (var p in game.Periods)
            p.UpdatePeriod(_time.Now);
        game.Periods.SetFirstPeriod();
        return game;
    }

    private void EnableSwapAtHalfTime()
    {
        var field = typeof(Score).GetField("_swapSecondaryAtHalfTime", BindingFlags.NonPublic | BindingFlags.Instance)!;
        field.SetValue(_score, true);
    }

    [Fact]
    public void SecondarySwapped_WhenAtHalfTimeAndSwapEnabled_SwapsTeams()
    {
        var game = CreateGame(
            ("Period 1", _time.Now, _time.Now.AddMinutes(10)),
            ("Period 2", _time.Now.AddMinutes(12), _time.Now.AddMinutes(22))
        );
        game.Team1 = "Team A";
        game.Team2 = "Team B";

        _score.Games.Add(game);
        _score.StartGames();
        EnableSwapAtHalfTime();

        _time.AdvanceSeconds(10 * 60 + 1);

        _score.SecondarySwapped.Should().BeTrue();
        _score.SecondaryCurrentGame.Swapped.Should().BeTrue();
        _score.SecondaryCurrentGame.Team1.Should().Be("Team B");
        _score.SecondaryCurrentGame.Team2.Should().Be("Team A");
    }

    [Fact]
    public void SecondarySwapped_WhenSwapDisabled_DoesNotSwap()
    {
        var game = CreateGame(
            ("Period 1", _time.Now, _time.Now.AddMinutes(10)),
            ("Period 2", _time.Now.AddMinutes(12), _time.Now.AddMinutes(22))
        );
        game.Team1 = "Team A";
        game.Team2 = "Team B";

        _score.Games.Add(game);
        _score.SwapSecondaryAtHalfTime = false;
        _score.StartGames();

        _time.AdvanceSeconds(10 * 60 + 1);

        _score.SecondarySwapped.Should().BeFalse();
        _score.SecondaryCurrentGame.Swapped.Should().BeFalse();
        _score.SecondaryCurrentGame.Team1.Should().Be("Team A");
        _score.SecondaryCurrentGame.Team2.Should().Be("Team B");
    }

    [Fact]
    public void CurrentOrEndedGame_StaysOnEndedGameThenTransitions_SixtySecondsAfterEnd()
    {
        var game1 = CreateGame(
            ("Period 1", _time.Now, _time.Now.AddMinutes(10))
        );
        game1.Team1 = "Team A";
        game1.Team2 = "Team B";

        var game2 = CreateGame(
            ("Period 1", _time.Now.AddMinutes(15), _time.Now.AddMinutes(25))
        );
        game2.Team1 = "Team C";
        game2.Team2 = "Team D";

        _score.Games.Add(game1);
        _score.Games.Add(game2);
        _score.StartGames();

        _time.AdvanceSeconds(10 * 60 + 1);

        _score.CurrentGame.Should().BeSameAs(game2);
        _score.CurrentOrEndedGame.Should().BeSameAs(game1);

        _time.AdvanceSeconds(60);

        _score.CurrentOrEndedGame.Should().BeSameAs(game2);
    }

    [Fact]
    public void CurrentOrEndedGame_TransitionsThirtySecondsBeforeNextGameStarts_WhenGapUnderNinety()
    {
        var game1 = CreateGame(
            ("Period 1", _time.Now, _time.Now.AddMinutes(10))
        );
        game1.Team1 = "Team A";
        game1.Team2 = "Team B";

        var game2 = CreateGame(
            ("Period 1", _time.Now.AddMinutes(10).AddSeconds(45), _time.Now.AddMinutes(20).AddSeconds(45))
        );
        game2.Team1 = "Team C";
        game2.Team2 = "Team D";

        _score.Games.Add(game1);
        _score.Games.Add(game2);
        _score.StartGames();

        _time.AdvanceSeconds(10 * 60 + 1);

        _score.CurrentOrEndedGame.Should().BeSameAs(game1);

        _time.AdvanceSeconds(15);

        _score.CurrentOrEndedGame.Should().BeSameAs(game2);
    }

    [Fact]
    public void SecondaryCurrentOrEndedGame_SwapsAtHalfTime_WhenSwapEnabled()
    {
        var game = CreateGame(
            ("Period 1", _time.Now, _time.Now.AddMinutes(10)),
            ("Period 2", _time.Now.AddMinutes(12), _time.Now.AddMinutes(22))
        );
        game.Team1 = "Team A";
        game.Team2 = "Team B";

        _score.Games.Add(game);
        _score.StartGames();
        EnableSwapAtHalfTime();

        _time.AdvanceSeconds(10 * 60 + 1);

        _score.SecondaryCurrentOrEndedGame.Swapped.Should().BeTrue();
        _score.SecondaryCurrentOrEndedGame.Team1.Should().Be("Team B");
    }

    [Fact]
    public void SecondaryCurrentOrEndedGame_TransitionsAfterGameEnds_WithSwap()
    {
        var game1 = CreateGame(
            ("Period 1", _time.Now, _time.Now.AddMinutes(10))
        );
        game1.Team1 = "Team A";
        game1.Team2 = "Team B";

        var game2 = CreateGame(
            ("Period 1", _time.Now.AddMinutes(15), _time.Now.AddMinutes(25))
        );
        game2.Team1 = "Team C";
        game2.Team2 = "Team D";

        _score.Games.Add(game1);
        _score.Games.Add(game2);
        _score.StartGames();
        EnableSwapAtHalfTime();

        _time.AdvanceSeconds(10 * 60 + 1);

        _score.CurrentOrEndedGame.Should().BeSameAs(game1);
        _score.SecondaryCurrentOrEndedGame.Game.Should().BeSameAs(game1);
        _score.SecondaryCurrentOrEndedGame.Swapped.Should().BeFalse();

        _time.AdvanceSeconds(60);

        _score.CurrentOrEndedGame.Should().BeSameAs(game2);
        _score.SecondaryCurrentOrEndedGame.Game.Should().BeSameAs(game2);
        _score.SecondaryCurrentOrEndedGame.Swapped.Should().BeFalse();
    }

    [Fact]
    public void NextGame_WhenNoMoreGames_CurrentGameBecomesNull()
    {
        var game = CreateGame(
            ("Period 1", _time.Now, _time.Now.AddMinutes(10))
        );
        game.Team1 = "Team A";
        game.Team2 = "Team B";

        _score.Games.Add(game);
        _score.StartGames();

        _time.AdvanceSeconds(10 * 60 + 1);

        _score.CurrentGame.Should().BeNull();
        _score.CurrentOrEndedGame.Should().BeSameAs(game);
    }
}
