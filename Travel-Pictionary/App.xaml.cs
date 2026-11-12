using Travel_Pictionary.Services;

namespace Travel_Pictionary
{
    public partial class App : Application
    {
        public GameCoordinator GameCoordinator { get; } = new(new PromptRepository());

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}