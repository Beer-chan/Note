using Microsoft.Extensions.DependencyInjection;

namespace Note
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            var navPage = new NavigationPage(new FirstPage());
            MainPage = navPage;
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Width = 1175;
            window.Height = 740;

            return window;
        }

    }
}