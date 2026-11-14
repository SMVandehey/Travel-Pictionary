using CommunityToolkit.Maui.Core.Platform;

namespace Travel_Pictionary;

public partial class PlayerSelectPage : ContentPage
{
    private int playerCount = 2;

    public PlayerSelectPage()
    {
        InitializeComponent();
        ApplyPreviousNames();
    }

    private App GameApp => (App)Application.Current!;

    private void ApplyPreviousNames()
    {
        var previousNames = GameApp.GameCoordinator.PreviousPlayerNames;
        var entries = GetEntries();

        for (var index = 0; index < previousNames.Count && index < entries.Count; index++)
        {
            entries[index].Text = previousNames[index];
        }

        if (previousNames.Count >= 3)
        {
            playerCount = Math.Min(previousNames.Count, 4);
            Player3Entry.IsVisible = playerCount >= 3;
            Player4Entry.IsVisible = playerCount >= 4;
        }

        UpdatePlayerCountButtons();
        UpdateReadyState();
    }

    private void OnPlayerCountClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string value || !int.TryParse(value, out var count))
        {
            return;
        }

        playerCount = count;
        Player3Entry.IsVisible = count >= 3;
        Player4Entry.IsVisible = count >= 4;
        UpdatePlayerCountButtons();
        UpdateReadyState();
    }

    private void OnNameChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateReadyState();
    }

    private async void OnReadyClicked(object? sender, EventArgs e)
    {
        var names = GetEntries()
            .Take(playerCount)
            .Select(entry => entry.Text?.Trim() ?? string.Empty)
            .ToArray();

        if (!AreNamesValid(names))
        {
            UpdateReadyState();
            return;
        }

        try
        {
            foreach (var entry in GetEntries())
            {
                entry.Unfocus();
            }

            await Player1Entry.HideKeyboardAsync();
            await GameApp.GameCoordinator.StartNewGameAsync(names);
            await Shell.Current.GoToAsync(nameof(TurnTransitionPage));
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException)
        {
            await DisplayAlertAsync("Prompt List Error", exception.Message, "OK");
        }
    }

    private void UpdateReadyState()
    {
        var names = GetEntries()
            .Take(playerCount)
            .Select(entry => entry.Text?.Trim() ?? string.Empty)
            .ToArray();

        ReadyButton.IsEnabled = AreNamesValid(names);
    }

    private void UpdatePlayerCountButtons()
    {
        var selectedColor = (Color)Application.Current!.Resources["GameBlue"];
        var unselectedColor = (Color)Application.Current.Resources["GameOrange"];

        foreach (var (button, count) in new[]
        {
            (PlayerCount2Button, 2),
            (PlayerCount3Button, 3),
            (PlayerCount4Button, 4)
        })
        {
            button.BackgroundColor = playerCount == count ? selectedColor : unselectedColor;
            button.BorderWidth = playerCount == count ? 5 : 3;
        }
    }

    private IReadOnlyList<Entry> GetEntries()
    {
        return [Player1Entry, Player2Entry, Player3Entry, Player4Entry];
    }

    private static bool AreNamesValid(IReadOnlyList<string> names)
    {
        return names.Count >= 2 &&
               names.All(name => !string.IsNullOrWhiteSpace(name)) &&
               names.Distinct(StringComparer.OrdinalIgnoreCase).Count() == names.Count;
    }
}
