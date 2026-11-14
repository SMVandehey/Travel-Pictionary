using Travel_Pictionary.Models;

namespace Travel_Pictionary.Services;

public sealed class GameCoordinator
{
    private readonly PromptRepository promptRepository;
    private readonly Random random;
    private IReadOnlyList<string> prompts = Array.Empty<string>();

    public GameCoordinator(PromptRepository promptRepository, Random? random = null)
    {
        this.promptRepository = promptRepository;
        this.random = random ?? Random.Shared;
    }

    public GameSession? CurrentSession { get; private set; }

    public IReadOnlyList<string> PreviousPlayerNames { get; private set; } = Array.Empty<string>();

    public async Task<GameSession> StartNewGameAsync(
        IReadOnlyList<string> playerNames,
        CancellationToken cancellationToken = default)
    {
        prompts = await promptRepository.LoadPromptsAsync(cancellationToken);
        var startingPlayerIndex = random.Next(playerNames.Count);
        PreviousPlayerNames = playerNames.Select(name => name.Trim()).ToArray();
        CurrentSession = new GameSession(playerNames, startingPlayerIndex);
        return CurrentSession;
    }

    public string SelectPrompt()
    {
        var session = GetCurrentSession();
        var availablePrompts = prompts
            .Where(prompt => !session.UsedPrompts.Contains(prompt))
            .ToArray();

        if (availablePrompts.Length == 0)
        {
            throw new InvalidOperationException("There are no unused prompts remaining for this game.");
        }

        var prompt = availablePrompts[random.Next(availablePrompts.Length)];
        if (!session.TryUsePrompt(prompt))
        {
            throw new InvalidOperationException("The selected prompt could not be recorded.");
        }

        session.Phase = GamePhase.WordPrompt;
        return prompt;
    }

    public bool CompleteTurnWithCorrectGuess(int guessingPlayerIndex)
    {
        var session = GetCurrentSession();
        if (session.IsTurnCompleted)
        {
            return false;
        }

        if (guessingPlayerIndex < 0 || guessingPlayerIndex >= session.Players.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(guessingPlayerIndex));
        }

        if (guessingPlayerIndex == session.CurrentPlayerIndex)
        {
            throw new ArgumentException("The drawing player cannot guess their own prompt.", nameof(guessingPlayerIndex));
        }

        session.CompleteTurn();
        session.CurrentPlayer.AddPoints(1);
        session.Players[guessingPlayerIndex].AddPoints(1);
        session.AdvanceToNextTurn();
        return true;
    }

    public bool CompleteTurnWithTimeout()
    {
        var session = GetCurrentSession();
        if (session.IsTurnCompleted)
        {
            return false;
        }

        session.CompleteTurn();
        session.AdvanceToNextTurn();
        return true;
    }

    public void EndGame()
    {
        GetCurrentSession().EndGame();
    }

    public GameResult GetResult()
    {
        return GetCurrentSession().CreateResult();
    }

    private GameSession GetCurrentSession()
    {
        return CurrentSession ?? throw new InvalidOperationException("A game has not been started.");
    }
}
