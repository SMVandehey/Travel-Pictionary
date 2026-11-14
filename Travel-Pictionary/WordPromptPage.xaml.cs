namespace Travel_Pictionary;

public partial class WordPromptPage : ContentPage
{
    private CancellationTokenSource? cancellationTokenSource;
    private bool hasNavigated;

    public WordPromptPage()
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
        _ = RunPromptAsync(cancellationTokenSource.Token);
    }

    protected override void OnDisappearing()
    {
        cancellationTokenSource?.Cancel();
        base.OnDisappearing();
    }

    private async void OnLetsGoClicked(object? sender, EventArgs e)
    {
        if (hasNavigated)
        {
            return;
        }

        hasNavigated = true;
        cancellationTokenSource?.Cancel();
        await Shell.Current.GoToAsync(nameof(DrawingPage));
    }

    private async Task RunPromptAsync(CancellationToken cancellationToken)
    {
        try
        {
            var prompt = GameApp.GameCoordinator.SelectPrompt();
            PromptLabel.FormattedText = new FormattedString
            {
                Spans =
                {
                    new Span { Text = "Your picture is " },
                    new Span
                    {
                        Text = $"{prompt}!",
                        TextColor = (Color)Application.Current!.Resources["GameBlue"]
                    }
                }
            };

            for (var seconds = 15; seconds > 0; seconds--)
            {
                CountdownLabel.Text = seconds.ToString();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            if (!hasNavigated && !cancellationToken.IsCancellationRequested)
            {
                hasNavigated = true;
                await Shell.Current.GoToAsync(nameof(DrawingPage));
            }
        }
        catch (InvalidOperationException exception)
        {
            await DisplayAlertAsync("Prompt Error", exception.Message, "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}
