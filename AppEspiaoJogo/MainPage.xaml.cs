using AppEspiaoJogo.Common;
using AppEspiaoJogo.Enums;
using AppEspiaoJogo.Game;
using AppEspiaoJogo.Services;

#if ANDROID
using Android.Content;
using AppEspiaoJogo.Platforms.Android.Services;
#endif

#pragma warning disable CS0618

namespace AppEspiaoJogo
{
    public partial class MainPage : ContentPage
    {
        ClientSocketService _clientSocketService;
        private bool _isConnectButtonClickable = true;

#if ANDROID
            public static Context? _context;
            public static Intent? _intent;
#endif

        public MainPage()
        {
            InitializeComponent();
            SetClientInitialState();

            #if ANDROID
                _context = Android.App.Application.Context;
                _intent = new Intent(_context, typeof(ClientSocketForegroundService));
            #endif

            MessagingCenter.Subscribe<object, (string title, string message)>(this, "DisplayAlert", async (sender, data) =>
            {
                await DisplayAlert(data.title, data.message, "OK");
            });

            MessagingCenter.Subscribe<object, ClientStateEnum>(this, "SetClientState", (sender, state) =>
            {
                switch(state)
                {
                    case ClientStateEnum.Initial:
                        SetClientInitialState();
                        break;
                    case ClientStateEnum.Running:
                        SetClientRunningState();
                        break;
                }
            });

            MessagingCenter.Subscribe<object, GameContent>(this, "SetClientGameData", (sender, gameData) =>
            {
                GameNumber.Text = gameData.Number.ToString();
                GameWord.Text = gameData.Word.ToString();
                GameWord.TextColor = GameWord.Text == "ESPIÃO" ? Colors.Red : GameWord.TextColor = Colors.White;
            });

            MessagingCenter.Subscribe<object, string>(this, "ConnectionError", async (sender, msg) =>
            { 
                #if ANDROID
                    Android.App.Application.Context.StopService(_intent);
                #endif

                await DisplayAlert("Erro", msg, "OK");
                SetClientInitialState();
            });

            _clientSocketService = new ClientSocketService();
        }

        private void SetClientRunningState()
        {
            DeviceIpEntry.IsVisible = false;
            ConnectButton.Text = "Desconectar";
            ConnectLabel.IsVisible = false;
            GameNumber.Text = "0";
            GameWord.Text = "-";
            PasteButton.IsVisible = false;
        }

        private void SetClientInitialState()
        {
            DeviceIpEntry.IsVisible = true;
            ConnectButton.Text = "Conectar";
            ConnectLabel.IsVisible = true;
            PasteButton.IsVisible = true;
            GameNumber.Text = "0";
            GameWord.Text = "-";
            GameWord.TextColor = Colors.White;
        }

        private async void ConnectDevice_Clicked(object sender, EventArgs e)
        {
            if (!_isConnectButtonClickable) 
                return;

            _isConnectButtonClickable = false;
            ConnectButton.IsEnabled = false;

            if (NetworkCommon.IsClientConnected())
            {
                await _clientSocketService.Disconnect();
                SetClientInitialState();

#if ANDROID
                Android.App.Application.Context.StopService(_intent);
#endif

                await Task.Delay(2000);

                _isConnectButtonClickable = true;
                ConnectButton.IsEnabled = true;
                return;
            }

#if WINDOWS
                await _clientSocketService.ConnectDevice(DeviceIpEntry.Text);
#endif

#if ANDROID
            _intent.PutExtra("ServerIp", DeviceIpEntry.Text);
            _context.StartService(_intent);
#endif

            await Task.Delay(2000);

            _isConnectButtonClickable = true;
            ConnectButton.IsEnabled = true;
        }

        private async void OnHostButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HostPage());
        }

        private async void Paste_Clicked(object sender, EventArgs e)
        {
            if (Clipboard.Default.HasText)
            {
                var ip = await Clipboard.Default.GetTextAsync();
                DeviceIpEntry.Text = ip;
            }
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {            
            #if ANDROID                
                if (ForegroundServicesManager.ActiveServerService != null)
                    Android.App.Application.Context.StopService(ForegroundServicesManager.ActiveServerService);

                if(NetworkCommon.ServerIsRunning)
                {
                    NetworkCommon.StopServer();
                    DisplayAlert("Aviso", "Encerrando servidor.", "OK");
                }
            #endif
            base.OnNavigatedTo(args);
        }

        private async void OnPageDoubleTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushModalAsync(new LocationsPage());
        }
    }
}
