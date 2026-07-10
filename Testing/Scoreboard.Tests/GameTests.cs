using FluentAssertions;

namespace Scoreboard.Tests;

public class GameTests
{
    private static Game CreateEndedGame()
    {
        var game = new Game();
        game.Periods.Add(new GamePeriod
        {
            Name = "Period 1",
            StartTime = DateTime.Now.AddMinutes(-10),
            EndTime = DateTime.Now.AddMinutes(-5)
        });
        game.Periods.SetFirstPeriod();
        game.Periods[0].Status = GamePeriodStatus.Ended;
        return game;
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var game = new Game();

        game.Team1Score.Should().Be(0);
        game.Team2Score.Should().Be(0);
        game.HasStarted.Should().BeFalse();
        game.HasEnded.Should().BeFalse();
        game.HasCompleted.Should().BeFalse();
    }

    [Fact]
    public void Team1Score_WhenSet_NotifiesPropertyChanged()
    {
        var game = new Game();
        var changedProperties = new List<string>();
        game.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        game.Team1Score = 3;

        changedProperties.Should().Contain("Team1Score");
    }

    [Fact]
    public void Team2Score_WhenSet_NotifiesPropertyChanged()
    {
        var game = new Game();
        var changedProperties = new List<string>();
        game.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        game.Team2Score = 2;

        changedProperties.Should().Contain("Team2Score");
    }

    [Fact]
    public void Team1Name_WhenSet_NotifiesPropertyChanged()
    {
        var game = new Game();
        var changedProperties = new List<string>();
        game.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        game.Team1 = "Team Alpha";

        changedProperties.Should().Contain("Team1");
    }

    [Fact]
    public void Team2Name_WhenSet_NotifiesPropertyChanged()
    {
        var game = new Game();
        var changedProperties = new List<string>();
        game.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        game.Team2 = "Team Beta";

        changedProperties.Should().Contain("Team2");
    }

    [Fact]
    public void Result_WhenScoresDifferAndEnded_ReturnsWinningTeam()
    {
        var game = CreateEndedGame();
        game.Team1Score = 3;
        game.Team2Score = 1;

        game.CalculateResult();

        game.Result.Should().Be(GameResult.Team1);
    }

    [Fact]
    public void Result_WhenScoresEqualAndEnded_ReturnsDraw()
    {
        var game = CreateEndedGame();
        game.Team1Score = 2;
        game.Team2Score = 2;

        game.CalculateResult();

        game.Result.Should().Be(GameResult.Draw);
    }

    [Fact]
    public void Result_WhenTeam2Wins_ReturnsTeam2()
    {
        var game = CreateEndedGame();
        game.Team1Score = 1;
        game.Team2Score = 4;

        game.CalculateResult();

        game.Result.Should().Be(GameResult.Team2);
    }

    [Fact]
    public void Result_WhenNotEnded_ReturnsNone()
    {
        var game = new Game();
        game.Team1Score = 3;
        game.Team2Score = 1;

        game.CalculateResult();

        game.Result.Should().Be(GameResult.None);
    }

    [Fact]
    public void ClearStatus_ResetsScoresAndEvents()
    {
        var game = new Game();
        game.Team1Score = 3;
        game.Team2Score = 2;
        game.LogEvent("Goal");

        game.ClearStatus();

        game.Team1Score.Should().Be(0);
        game.Team2Score.Should().Be(0);
        game.Result.Should().Be(GameResult.None);
        game.GameEvents.Should().BeEmpty();
    }

    [Fact]
    public void ToJson_ReturnsValidJson()
    {
        var game = new Game
        {
            Team1 = "Team A",
            Team2 = "Team B",
            Team1Score = 5,
            Team2Score = 3
        };

        var json = game.ToJson();

        json.Should().Contain("Team A");
        json.Should().Contain("Team B");
        json.Should().Contain("5");
        json.Should().Contain("3");
    }

    [Fact]
    public void Pool_WhenSet_NotifiesPropertyChanged()
    {
        var game = new Game();
        var changedProperties = new List<string>();
        game.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        game.Pool = "Group A";

        changedProperties.Should().Contain("Pool");
    }

    [Fact]
    public void LogEvent_AddsEventToList()
    {
        var game = new Game();

        game.LogEvent("Test Event");

        game.GameEvents.Should().HaveCount(1);
        game.GameEvents[0].EventType.Should().Be("Test Event");
    }
}
