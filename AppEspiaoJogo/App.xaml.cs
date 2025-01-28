using System.Net;

namespace AppEspiaoJogo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            if (DeviceInfo.Platform != DevicePlatform.WinUI)
            {
                return window;
            }

            const int newWidth = 600;
            const int newHeight = 750;

            window.Width = newWidth;
            window.Height = newHeight;

            return window;
        }

        protected override void OnResume()
        {
            MessagingCenter.Send<object>(this, "ResumeApp");            
        }
    }
}
