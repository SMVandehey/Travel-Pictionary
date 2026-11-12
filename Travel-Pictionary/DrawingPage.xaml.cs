namespace Travel_Pictionary;

public partial class DrawingPage : ContentPage
{
    private CancellationTokenSource? cancellationTokenSource;
    private int turnCompletionState;

    public DrawingPage()
    {
        InitializeComponent();
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
    }

    protected override bool OnBackButtonPressed() => true;

    private App GameApp => (App)Application.Current!;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        turnCompletionState = 0;
        BuildGuessButtons();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        _ = RunTimerAsync(cancellationTokenSource.Token);
    }

    protected override void OnDisappearing()
    {
        cancellationTokenSource?.Cancel();
        base.OnDisappearing();
    }

    private void BuildGuessButtons()
    {
        GuessButtons.Clear();
        var session = GameApp.GameCoordinator.CurrentSession;
        if (session is null)
        {
            return;
        }

        foreach (var player in session.Players.Where(player => player.PlayerIndex != session.CurrentPlayerIndex))
        {
            var button = new Button
            {
                Text = $"{player.Name} got it!",
                CommandParameter = player.PlayerIndex
            };
            button.Clicked += OnGuessButtonClicked;
            GuessButtons.Add(button);
        }
    }

    private async void OnGuessButtonClicked(object? sender, EventArgs e)
    {
        if (Interlocked.CompareExchange(ref turnCompletionState, 1, 0) != 0)
        {
            return;
        }

        SetGuessButtonsEnabled(false);
        cancellationTokenSource?.Cancel();

        if (sender is Button { CommandParameter: int playerIndex })
        {
            GameApp.GameCoordinator.CompleteTurnWithCorrectGuess(playerIndex);
        }

        await NavigateAfterTurnAsync();
    }

    private async Task RunTimerAsync(CancellationToken cancellationToken)
    {
        for (var seconds = 60; seconds > 0; seconds--)
        {
            TimerLabel.Text = $"{seconds}s";
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        if (Interlocked.CompareExchange(ref turnCompletionState, 1, 0) == 0)
        {
            SetGuessButtonsEnabled(false);
            GameApp.GameCoordinator.CompleteTurnWithTimeout();
            await NavigateAfterTurnAsync();
        }
    }

    private async Task NavigateAfterTurnAsync()
    {
        var session = GameApp.GameCoordinator.CurrentSession;
        if (session?.Phase == Models.GamePhase.Summary)
        {
            await Shell.Current.GoToAsync(nameof(SummaryPage));
        }
        else
        {
            await Shell.Current.GoToAsync(nameof(TurnTransitionPage));
        }
    }

    private void SetGuessButtonsEnabled(bool isEnabled)
    {
        foreach (var button in GuessButtons.Children.OfType<Button>())
        {
            button.IsEnabled = isEnabled;
        }
    }
}
