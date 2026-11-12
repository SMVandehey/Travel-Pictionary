namespace Travel_Pictionary.Models;

public sealed record GameResult(
    IReadOnlyList<Player> Players,
    int HighestScore,
    IReadOnlyList<Player> Winners)
{
    public bool IsTie => Winners.Count > 1;
}
