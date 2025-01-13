using AppEspiaoJogo.Common;
using AppEspiaoJogo.Enums;
using AppEspiaoJogo.Game;
using AppEspiaoJogo.Services;

#pragma warning disable CS0618

namespace AppEspiaoJogo
{
    public partial class MainPage : ContentPage
    {
        ClientSocketService _clientSocketService;
        private bool _isConnectButtonClickable = true;

        public MainPage()
        {
            _clientSocketService = new ClientSocketService();

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
#if ANDROID
                ForegroundServicesManager.StopClient();
#endif
                await NetworkCommon.StopClient();

                SetClientInitialState();

                await Task.Delay(2000);

                _isConnectButtonClickable = true;
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
            await Navigation.PushModalAsync(new LocationsPage());
        }
    }
}
