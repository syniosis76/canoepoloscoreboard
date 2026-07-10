using FluentAssertions;

namespace Scoreboard.Tests;

public class SwappedGameTests
{
    private static (Game game, Score score) CreateTestGame()
    {
        var game = new Game
        {
            Team1 = "Team A",
            Team2 = "Team B",
            Team1Score = 5,
            Team2Score = 3,
            Team1Flag = "us",
            Team2Flag = "gb",
            Team1Color = "#FF0000",
            Team2Color = "#0000FF"
        };
        var score = new Score(new TestDoubles.FakeTimeService());
        return (game, score);
    }

    [Fact]
    public void Constructor_WhenNotSwapped_PropertiesPassThrough()
    {
        var (game, score) = CreateTestGame();
        var swapped = new SwappedGame(game, false, score);

        swapped.Team1.Should().Be("Team A");
        swapped.Team2.Should().Be("Team B");
        swapped.Team1Score.Should().Be(5);
        swapped.Team2Score.Should().Be(3);
        swapped.Team1Flag.Should().Be("us");
        swapped.Team2Flag.Should().Be("gb");
        swapped.Team1Color.Should().Be("#FF0000");
        swapped.Team2Color.Should().Be("#0000FF");
    }

    [Fact]
    public void Constructor_WhenSwapped_PropertiesAreExchanged()
    {
        var (game, score) = CreateTestGame();
        var swapped = new SwappedGame(game, true, score);

        swapped.Team1.Should().Be("Team B");
        swapped.Team2.Should().Be("Team A");
        swapped.Team1Score.Should().Be(3);
        swapped.Team2Score.Should().Be(5);
        swapped.Team1Flag.Should().Be("gb");
        swapped.Team2Flag.Should().Be("us");
        swapped.Team1Color.Should().Be("#0000FF");
        swapped.Team2Color.Should().Be("#FF0000");
    }

    [Fact]
    public void ToJson_WhenSwapped_ContainsSwappedValues()
    {
        var (game, score) = CreateTestGame();
        var swapped = new SwappedGame(game, true, score);

        var json = swapped.ToJson();

        json.Should().Contain("Team B");
        json.Should().Contain("Team A");
    }

    [Fact]
    public void PropertyChanged_WhenNotSwapped_ForwardsWithoutSwap()
    {
        var (game, score) = CreateTestGame();
        var swapped = new SwappedGame(game, false, score);
        var forwardedProperties = new List<string>();
        swapped.PropertyChanged += (_, e) => forwardedProperties.Add(e.PropertyName!);

        game.Team1 = "New Team A";

        forwardedProperties.Should().Contain("Team1");
        swapped.Team1.Should().Be("New Team A");
    }

    [Fact]
    public void PropertyChanged_WhenSwapped_ForwardsWithSwap()
    {
        var (game, score) = CreateTestGame();
        var swapped = new SwappedGame(game, true, score);
        var forwardedProperties = new List<string>();
        swapped.PropertyChanged += (_, e) => forwardedProperties.Add(e.PropertyName!);

        game.Team1 = "New Team A";

        forwardedProperties.Should().Contain("Team2");
        swapped.Team2.Should().Be("New Team A");
    }
}
