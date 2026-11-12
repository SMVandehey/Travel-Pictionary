namespace Travel_Pictionary
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnNewGameClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(PlayerSelectPage));
        }
    }
}
 