namespace Travel_Pictionary.Models;

public sealed class GameSession
{
    public const int MinimumPlayers = 2;
    public const int MaximumPlayers = 4;
    public const int TotalRounds = 3;

    private readonly List<Player> players;
    private readonly HashSet<string> usedPrompts = new(StringComparer.OrdinalIgnoreCase);

    public GameSession(IReadOnlyList<string> playerNames, int startingPlayerIndex)
    {
        ArgumentNullException.ThrowIfNull(playerNames);

        if (playerNames.Count is < MinimumPlayers or > MaximumPlayers)
        {
            throw new ArgumentOutOfRangeException(nameof(playerNames), $"A game requires between {MinimumPlayers} and {MaximumPlayers} players.");
        }

        var normalizedNames = playerNames
            .Select(name => name.Trim())
            .ToArray();

        if (normalizedNames.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Player names cannot be empty.", nameof(playerNames));
        }

        if (normalizedNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalizedNames.Length)
        {
            throw new ArgumentException("Player names must be unique.", nameof(playerNames));
        }

        if (startingPlayerIndex < 0 || startingPlayerIndex >= normalizedNames.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(startingPlayerIndex));
        }

        players = normalizedNames
            .Select((name, index) => new Player(name, index))
            .ToList();

        StartingPlayerIndex = startingPlayerIndex;
        CurrentPlayerIndex = startingPlayerIndex;
        CurrentRound = 1;
        Phase = GamePhase.TurnTransition;
    }

    public IReadOnlyList<Player> Players => players;

    public IReadOnlySet<string> UsedPrompts => usedPrompts;

    public int StartingPlayerIndex { get; }

    public int CurrentPlayerIndex { get; private set; }

    public Player CurrentPlayer => players[CurrentPlayerIndex];

    public int CurrentRound { get; private set; }

    public string? CurrentPrompt { get; private set; }

    public GamePhase Phase { get; internal set; }

    public bool IsTurnCompleted { get; internal set; }

    public bool IsGameComplete => CurrentRound > TotalRounds;

    public bool TryUsePrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return false;
        }

        var normalizedPrompt = prompt.Trim();
        if (!usedPrompts.Add(normalizedPrompt))
        {
            return false;
        }

        CurrentPrompt = normalizedPrompt;
        return true;
    }

    public void CompleteTurn()
    {
        if (IsTurnCompleted)
        {
            return;
        }

        IsTurnCompleted = true;
    }

    public void AdvanceToNextTurn()
    {
        if (!IsTurnCompleted)
        {
            throw new InvalidOperationException("The current turn must be completed before advancing.");
        }

        IsTurnCompleted = false;
        CurrentPrompt = null;

        if (CurrentPlayerIndex == (StartingPlayerIndex + players.Count - 1) % players.Count)
        {
            CurrentRound++;
        }

        if (!IsGameComplete)
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % players.Count;
            Phase = GamePhase.TurnTransition;
        }
        else
        {
            Phase = GamePhase.Summary;
        }
    }

    public GameResult CreateResult()
    {
        if (!IsGameComplete)
        {
            throw new InvalidOperationException("A result is only available after the final round.");
        }

        var highestScore = players.Max(player => player.Score);
        var winners = players
            .Where(player => player.Score == highestScore)
            .ToArray();

        return new GameResult(players.AsReadOnly(), highestScore, winners);
    }
}
