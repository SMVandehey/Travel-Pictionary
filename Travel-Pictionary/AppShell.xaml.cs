namespace Travel_Pictionary
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(PlayerSelectPage), typeof(PlayerSelectPage));
            Routing.RegisterRoute(nameof(TurnTransitionPage), typeof(TurnTransitionPage));
            Routing.RegisterRoute(nameof(WordPromptPage), typeof(WordPromptPage));
            Routing.RegisterRoute(nameof(DrawingPage), typeof(DrawingPage));
            Routing.RegisterRoute(nameof(SummaryPage), typeof(SummaryPage));
        }
    }
}
