namespace Travel_Pictionary;

public partial class TurnTransitionPage : ContentPage
{
    private CancellationTokenSource? cancellationTokenSource;
    private bool hasNavigated;

    public TurnTransitionPage()
    {
        InitializeComponent();
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
    }

    protected override bool OnBackButtonPressed() => true;

    private App GameApp => (App)Application.Current!;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        hasNavigated = false;
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        _ = RunTransitionAsync(cancellationTokenSource.Token);
    }

    protected override void OnDisappearing()
    {
        cancellationTokenSource?.Cancel();
        base.OnDisappearing();
    }

    private async Task RunTransitionAsync(CancellationToken cancellationToken)
    {
        var session = GameApp.GameCoordinator.CurrentSession;
        if (session is null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        MessageLabel.Text = $"{session.CurrentPlayer.Name}'s turn! Prepare to draw!";

        for (var seconds = 7; seconds > 0; seconds--)
        {
            CountdownLabel.Text = seconds.ToString();
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        if (!hasNavigated && !cancellationToken.IsCancellationRequested)
        {
            hasNavigated = true;
            await Shell.Current.GoToAsync(nameof(WordPromptPage));
        }
    }
}
