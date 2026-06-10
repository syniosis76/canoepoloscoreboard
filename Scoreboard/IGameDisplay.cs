namespace Scoreboard
{
    public interface IGameDisplay
    {
        string Team1 { get; }
        string Team2 { get; }
        int Team1Score { get; }
        int Team2Score { get; }
        GamePeriodList Periods { get; }
        GameList Parent { get; }
        string ToJson();
    }
}
