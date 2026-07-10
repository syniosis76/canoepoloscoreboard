using System.Windows.Media;
using FluentAssertions;

namespace Scoreboard.Tests;

public class CardTests
{
    private static GameEvent MakeEvent(string eventType, string player, string team, string notes)
        => new(DateTime.Now, eventType, team, player, notes);

    [Fact]
    public void Constructor_SetsTime()
    {
        var card = new Card(120, MakeEvent("Green Card", "Player 1", "Team A", "Infringement"));

        card.Time.Should().Be(120);
    }

    [Fact]
    public void Time_WhenDecremented_NotifiesPropertyChanged()
    {
        var card = new Card(60, MakeEvent("Yellow Card", "Player 2", "Team B", "Late tackle"));
        var changedProperties = new List<string>();
        card.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        card.Time = 59;

        changedProperties.Should().Contain("Time");
        card.Time.Should().Be(59);
    }

    [Fact]
    public void GameEvent_ReturnsEventPassedToConstructor()
    {
        var gameEvent = MakeEvent("Red Card", "Player 3", "Team A", "Violent conduct");
        var card = new Card(300, gameEvent);

        card.GameEvent.Should().BeSameAs(gameEvent);
    }

    [Fact]
    public void DisplayColor_ForGreenCard_ReturnsGreen()
    {
        var card = new Card(60, MakeEvent("Green Card", "Player", "Team", "Good play"));

        var color = card.DisplayColor as SolidColorBrush;

        color.Should().NotBeNull();
        color!.Color.Should().Be(Color.FromArgb(0xFF, 0x3A, 0xC8, 0x2B));
    }

    [Fact]
    public void DisplayColor_ForYellowCard_ReturnsYellow()
    {
        var card = new Card(60, MakeEvent("Yellow Card", "Player", "Team", "Warning"));

        var color = card.DisplayColor as SolidColorBrush;

        color.Should().NotBeNull();
        color!.Color.Should().Be(Color.FromArgb(0xFF, 0xF7, 0xCA, 0x16));
    }

    [Fact]
    public void DisplayColor_ForRedCard_ReturnsRed()
    {
        var card = new Card(60, MakeEvent("Red Card", "Player", "Team", "Sent off"));

        var color = card.DisplayColor as SolidColorBrush;

        color.Should().NotBeNull();
        color!.Color.Should().Be(Color.FromArgb(0xFF, 0xD3, 0x26, 0x28));
    }

    [Fact]
    public void DisplayColor_ForEjectionRedCard_ReturnsRed()
    {
        var card = new Card(60, MakeEvent("Ejection Red Card", "Player", "Team", "Ejected"));

        var color = card.DisplayColor as SolidColorBrush;

        color.Should().NotBeNull();
        color!.Color.Should().Be(Color.FromArgb(0xFF, 0xD3, 0x26, 0x28));
    }

    [Fact]
    public void DisplayColor_ForUnknownEvent_ReturnsWhite()
    {
        var card = new Card(60, MakeEvent("Unknown Event", "Player", "Team", "Something"));

        var color = card.DisplayColor as SolidColorBrush;

        color.Should().NotBeNull();
        color!.Color.Should().Be(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
    }
}
