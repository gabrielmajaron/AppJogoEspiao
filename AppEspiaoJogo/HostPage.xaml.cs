using AppEspiaoJogo.Common;
using AppEspiaoJogo.Platforms.Android.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace AppEspiaoJogo;

public partial class HostPage : ContentPage
{
    ServerHubService _server;

    private readonly HubConnection _hubConnection;

    public HostPage()
    {
        _server = new();
        _hubConnection = ServiceLocator.GetService<HubConnection>();

        InitializeComponent();

        SetHostInitialState();

        // CreateMessagingCenterSubscriptions();
    }

    /*
    private void CreateMessagingCenterSubscriptions()
    {
        MessagingCenter.Subscribe<object, HostStateEnum>(this, "SetHostState", (sender, state) =>
        {
            switch (state)
            {
                case HostStateEnum.Initial:
                    SetHostInitialState();
                    break;
                case HostStateEnum.Running:
                    SetHostRunningState();
                    break;
            }
        });

        MessagingCenter.Subscribe<object, string>(this, "SetPlayersCount", (sender, count) =>
        {
            ConnectedPlayersCount.Text = count;
        });

        MessagingCenter.Subscribe<object, (GameContent, bool)>(this, "SetHostGameData", async (sender, data) =>
        {

            var gameContent = data.Item1;
            var isSpy = data.Item2;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                GameNumber.Text = gameContent.Number.ToString();
                GameWord.Text = isSpy ? "ESPIÃO" : gameContent.Word;
                GameWord.TextColor = isSpy ? Colors.Red : Colors.White;
            });
        });

        MessagingCenter.Subscribe<object, string>(this, "ServerStartError", async (sender, msg) =>
        {

            SetHostInitialState();
#if ANDROID
            ForegroundServicesManager.StopServer();
#endif
#if WINDOWS
                NetworkCommon.StopServer();
#endif
            await DisplayAlert("Erro", msg, "OK");
        });
    }*/

    private void SetHostRunningState()
    {
        StartServerButton.Text = "Encerrar";
        StartGameButton.Text = "Iniciar Jogo";
        StartGameButton.IsEnabled = true;
    }

    private void SetHostInitialState()
    {
        StartServerButton.Text = "Iniciar Servidor";
        ConnectedPlayersCount.Text = "0";
        GameWord.Text = "-";
        GameNumber.Text = "0";
        GameWord.TextColor = Colors.White;
        NextGameButton.IsVisible = false;
        StartGameButton.IsVisible = true;
        StartGameButton.Text = "Iniciar Jogo";
        StartGameButton.IsEnabled = true;
    }

    private void SetGameRunningState()
    {
        StartGameButton.Text = "Jogo iniciado";
        StartGameButton.IsEnabled = false;
        StartGameButton.IsVisible = false;
        NextGameButton.IsVisible = true;
    }

    private async void StartServer_Clicked(object sender, EventArgs e)
    {

        /*var localIp = NetworkCommon.GetLocalIpAddress();

        await Clipboard.Default.SetTextAsync(localIp);

        return;*/
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            bool answer = await DisplayAlert("Confirmação",
                    "Você tem certeza que deseja encerrar?",
                    "Sim",
                    "Não");

            if (answer)
            {
                await _server.StopServerAsync();                
                SetHostInitialState();
            }

            return;
        }

        var localIp = NetworkCommon.GetLocalIpAddress();

        if(localIp == "127.0.0.1")
            await DisplayAlert("Alerta", $"Certifique-se de estar conectado em uma rede local.", "OK");

        await Clipboard.Default.SetTextAsync(localIp);

        //await _server.SendMessageAsync("ex server name", "test");
        _server.StartServer();

    }

    private async void StartGame_Clicked(object sender, EventArgs e)
    {
        if (!await CanPlay())
            return;

        SetGameRunningState();
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            NextGame_Clicked(sender, e);
        });
    }

    
    private async void NextGame_Clicked(object sender, EventArgs e)
    {
        if (!await CanPlay())
            return;

        await _server.NextGame();
    }

    private async Task<bool> CanPlay()
    {
        if (_server == null)
        {
            await DisplayAlert("Alerta", $"Inicie o servidor antes", "OK");
            return false;
        }

        if (!_server.IsSuficientClientsConnected())
        {
            await DisplayAlert("Alerta", $"Necessário ao menos 3 jogadores (contando com você) para iniciar o jogo", "OK");
            return false;
        }
        return true;
    }

    private async void OnPageDoubleTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushModalAsync(new LocationsPage());
    }
}
