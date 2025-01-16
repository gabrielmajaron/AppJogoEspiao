using AppEspiaoJogo.Common;
using Microsoft.AspNetCore.SignalR.Client;

#pragma warning disable CS0618

namespace AppEspiaoJogo
{
    public partial class MainPage : ContentPage
    {
        private bool _isConnectButtonClickable = true;

        private readonly HubConnection _hubConnection;

        private readonly HostPage _hostPage;        

        public string Message { get; set; }

        public MainPage()
        {
            _hubConnection = ServiceLocator.GetService<HubConnection>();
            _hostPage = ServiceLocator.GetService<HostPage>();

            _hubConnection.On<string, string>("ReceiveMessage", async (user, message) =>
            {
                await DisplayAlert("Msg", message, "OK");
            });

            InitializeComponent();

            SetClientInitialState();

            //CreateMessagingCenterSubscriptions();
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
                SetClientRunningState();
            }
        }

        public async Task SendMessageAsync()
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendMessage", "Ex nome usuário", Message);
                Message = string.Empty;
            }
        }
        /*
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
        }*/

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

            if (_hubConnection.State == HubConnectionState.Disconnected)
                await ConnectAsync();
            else
                await _hubConnection.StopAsync();

            await Task.Delay(2000);

            _isConnectButtonClickable = true;
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

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                DisplayAlert("Aviso", "Encerrando servidor.", "OK");
                _hubConnection.StopAsync();
            }                

            base.OnNavigatedTo(args);
        }     

        private async void OnPageDoubleTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushModalAsync(new LocationsPage());
        }
    }
}
