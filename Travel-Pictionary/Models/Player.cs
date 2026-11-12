namespace Travel_Pictionary.Models;

public sealed class Player
{
    public Player(string name, int playerIndex)
    {
        Name = name;
        PlayerIndex = playerIndex;
    }

    public string Name { get; }

    public int PlayerIndex { get; }

    public int Score { get; private set; }

    public void AddPoints(int points)
    {
        if (points < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(points), points, "Points cannot be negative.");
        }

        Score += points;
    }
}
