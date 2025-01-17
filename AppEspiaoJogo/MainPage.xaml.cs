using AppEspiaoJogo.Common;
using AppEspiaoJogo.Enums;
using AppEspiaoJogo.Game;
using AppEspiaoJogo.Services;

#pragma warning disable CS0618

namespace AppEspiaoJogo
{
    public partial class MainPage : ContentPage
    {
        private bool _allowClicks = true;
        private readonly HostPage _hostPage;
        private readonly ClientSocketService _clientSocketService;
        private readonly LocationsPage _locationsPage;

        public MainPage()
        {
            _clientSocketService = ServiceLocator.GetService<ClientSocketService>();
            _hostPage = ServiceLocator.GetService<HostPage>();
            _locationsPage = ServiceLocator.GetService<LocationsPage>();

            InitializeComponent();

            SetClientInitialState();

            CreateMessagingCenterSubscriptions();
        }

        private void CreateMessagingCenterSubscriptions()
        {
            MessagingCenter.Subscribe<object, (string title, string message)>(this, "DisplayAlert", async (sender, data) =>
            {
                await DisplayAlert(data.title, data.message, "OK");
            });

            MessagingCenter.Subscribe<object, ClientStateEnum>(this, "SetClientState", (sender, state) =>
            {
                switch (state)
                {
                    case ClientStateEnum.Initial:
#if ANDROID
                ForegroundServicesManager.StopClient();
#endif
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
                ForegroundServicesManager.StopClient();
#endif
                await DisplayAlert("Erro", msg, "OK");
                SetClientInitialState();
            });

            MessagingCenter.Subscribe<object, string>(this, "Disconnected", async (sender, errorMsg) =>
            {
#if ANDROID
                try
                {
                    ForegroundServicesManager.StopClient();
                }
                catch (Exception _)
                { }                
#endif
                try
                {
                    ConnectDevice_Clicked(sender, null); // try to reconnect once
                }
                catch (Exception _)
                {
                    await DisplayAlert("Erro", errorMsg, "OK");
                }
            });
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
            if (!_allowClicks)
                return;

            _allowClicks = false;
            ConnectButton.IsEnabled = false;

            if (NetworkCommon.IsClientConnected())
            {
#if ANDROID
                ForegroundServicesManager.StopClient();
#endif
                await NetworkCommon.StopClient();

                SetClientInitialState();

                await Task.Delay(2000);

                _allowClicks = true;
                ConnectButton.IsEnabled = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(DeviceIpEntry.Text))
            {
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await DisplayAlert("Erro", "Insira um endereço IP válido.", "OK");                    
                });
            }
            else
            {
#if WINDOWS
            await _clientSocketService.ConnectDevice(DeviceIpEntry.Text);
#endif

#if ANDROID
            ForegroundServicesManager.StartClient(DeviceIpEntry.Text);
#endif
            }

            await Task.Delay(2000);

            _allowClicks = true;
            ConnectButton.IsEnabled = true;
        }

        private async void OnHostButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(_hostPage);
        }

        private async void Paste_Clicked(object sender, EventArgs e)
        {
            if (Clipboard.Default.HasText)
            {
                var ip = await Clipboard.Default.GetTextAsync();
                DeviceIpEntry.Text = ip;
            }
        }

#if ANDROID
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            if (NetworkCommon.ServerIsRunning)            
                DisplayAlert("Aviso", "Encerrando servidor.", "OK");

            ForegroundServicesManager.StopServer();
            base.OnNavigatedTo(args);
        }        
#endif

        private async void OnPageDoubleTapped(object sender, TappedEventArgs e)
        {
            if (!_allowClicks)
                return;

            _allowClicks = false;
            this.IsEnabled = false;

            await Navigation.PushModalAsync(_locationsPage);

            await Task.Delay(2000);

            _allowClicks = true;
            this.IsEnabled = true;
        }
    }
}
