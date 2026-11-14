namespace Travel_Pictionary;

public partial class DrawingPage : ContentPage
{
    private CancellationTokenSource? cancellationTokenSource;
    private int turnCompletionState;

    public DrawingPage()
    {
        InitializeComponent();
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
        DrawingCanvas.LineWidth = 6;
        BuildColorButtons();
    }

    private void BuildColorButtons()
    {
        var colors = new[]
        {
            Color.FromArgb("#111111"), Color.FromArgb("#E53935"), Color.FromArgb("#FB8C00"),
            Color.FromArgb("#FDD835"), Color.FromArgb("#43A047"), Color.FromArgb("#00ACC1"),
            Color.FromArgb("#1E88E5"), Color.FromArgb("#3949AB"), Color.FromArgb("#8E24AA"),
            Color.FromArgb("#F06292")
        };

        foreach (var color in colors)
        {
            var button = new Button
            {
                BackgroundColor = color,
                WidthRequest = 44,
                HeightRequest = 44,
                Padding = 0,
                CornerRadius = 22,
                CommandParameter = color
            };
            button.Clicked += OnColorButtonClicked;
            ColorsLayout.Add(button);
        }

        DrawingCanvas.LineColor = colors[0];
    }

    private void OnColorButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: Color color })
        {
            DrawingCanvas.LineColor = color;
        }
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
                CommandParameter = player.PlayerIndex,
                Style = (Style)Application.Current!.Resources["GameButton"]
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
