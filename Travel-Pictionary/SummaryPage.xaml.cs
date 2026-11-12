using Travel_Pictionary.Models;

namespace Travel_Pictionary;

public partial class SummaryPage : ContentPage
{
    public SummaryPage()
    {
        InitializeComponent();
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
    }

    protected override bool OnBackButtonPressed() => true;

    private App GameApp => (App)Application.Current!;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var result = GameApp.GameCoordinator.GetResult();
        ScoresLayout.Clear();

        foreach (var player in result.Players)
        {
            ScoresLayout.Add(new Label
            {
                Text = $"{player.Name}: {player.Score}",
                FontSize = 22,
                HorizontalTextAlignment = TextAlignment.Center
            });
        }

        WinnerLabel.Text = result.IsTie
            ? $"{string.Join(", ", result.Winners.Select(player => player.Name))} tie!"
            : $"{result.Winners[0].Name} wins!";
    }

    private async void OnNewGameClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PlayerSelectPage));
    }
}
