using AppEspiaoJogo.Common;
using AppEspiaoJogo.Game;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Internal;
using Microsoft.Extensions.Logging;



namespace AppEspiaoJogo.Platforms.Android.Services
{
    public class ServerHubService : Hub
    {
        private IWebHost _webHost;

        private int _gameNumber = 0;

        private readonly HubConnection _hubConnection;

        private static ConcurrentDictionary<string, string> _connectedClients = new ConcurrentDictionary<string, string>();

        public ServerHubService()
        {
            _hubConnection = ServiceLocator.GetService<HubConnection>();
            SetUpClientEvents();

        }

        // Iniciar o servidor

        public void StartServer()
        {
            try
            {
                var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSignalR();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<HubConnection>(provider =>
                    new HubConnectionBuilder()
                        .WithUrl("https://127.0.0.1:5001")
                        .Build());
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .ConfigureDefaults(args: null)
                .Build();

                var webHost = new WebHostBuilder()
                    .UseKestrel()
                    .UseUrls("http://localhost:5000")
                    .Build();

                webHost.RunAsync();
                Console.WriteLine("Servidor SignalR iniciado.");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        // Parar o servidor
        public void StopServer()
        {
            _webHost?.StopAsync();
            Console.WriteLine("Servidor SignalR parado.");
        }

        public void SetUpClientEvents()
        {
            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"{user}: {message}");
                //if (disconnect) (...)
            });
        }
        /*
        public async Task StartServerAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("Conexão com SignalR iniciada.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao tentar iniciar a conexão: {ex.Message}");
            }
        }*/

        public async Task StopServerAsync()
        {
            try
            {
                await _hubConnection.StopAsync();
                Console.WriteLine("Conexão com SignalR encerrada.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao tentar encerrar a conexão: {ex.Message}");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", "Server", message);
        }

        public async Task SendMessageAsync(string clientConnectionId, string message)
        {
            await Clients.Client(clientConnectionId).SendAsync("ReceiveMessage", "Server", message);
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            _connectedClients.TryAdd(connectionId, "Cliente");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _connectedClients.TryRemove(connectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task NextGame()
        {
            var newGame = new GameContent
            {
                Number = ++_gameNumber,
                Word = Locations.GetRandomLocation()
            };

            Random random = new Random();
            int spy = random.Next(_connectedClients.Count() + 1);

            if (IsHostSpy(spy))
            {
                await GenerateGameForHostBeingTheSpy(newGame);
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<object, (GameContent, bool)>(this, "SetHostGameData", (newGame, false));
            });

            await GenerateGameForClientBeingSpy(newGame, spy);
        }

        private async Task GenerateGameForClientBeingSpy(GameContent newGame, int spy)
        {
            var clientSpyConnectionId = _connectedClients.ElementAt(spy).Key;

            var clientsConnectionId = _connectedClients.Where(c => c.Key != clientSpyConnectionId).Select(c => c.Key).ToList();

            await BroadcastMessageAsync(clientsConnectionId, JsonSerializer.Serialize(newGame));

            newGame.Word = "ESPIÃO";

            var msg = JsonSerializer.Serialize(newGame);

            await SendMessageAsync(clientSpyConnectionId, msg);
        }

        private async Task GenerateGameForHostBeingTheSpy(GameContent newGame)
        {
            await BroadcastMessageAsync(_connectedClients.Select(c => c.Key).ToList(), JsonSerializer.Serialize(newGame));

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<object, (GameContent, bool)>(this, "SetHostGameData", (newGame, true));
            });
        }

        private bool IsHostSpy(int spy)
        {
            return spy == _connectedClients.Count();
        }

        private async Task BroadcastMessageAsync(List<string> clientsConnectionId, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (var clientConnectionId in clientsConnectionId)
            {
                try
                {
                    await SendMessageAsync(clientConnectionId, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                }
            }

            //RemoveFromConnectedClients(failedToSend);
        }

        public bool IsSuficientClientsConnected()
        {
            return _connectedClients.Count >= 2; 
        }

        /*
        private void RemoveFromConnectedClients(List<TcpClient> clients)
        {
            NetworkCommon.ConnectedClients.RemoveAll(clients.Contains);
            NetworkCommon.ConnectedClients.RemoveAll(client => !client.Connected);
            var connectedClientsCount = NetworkCommon.ConnectedClients.Count(c => c.Connected);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<object, string>(this, "SetPlayersCount", connectedClientsCount.ToString());
            });
        }

        private void RemoveFromConnectedClients(TcpClient client)
        {
            NetworkCommon.ConnectedClients.Remove(client);
            NetworkCommon.ConnectedClients.RemoveAll(client => !client.Connected);
            var connectedClientsCount = NetworkCommon.ConnectedClients.Count(c => c.Connected);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<object, string>(this, "SetPlayersCount", connectedClientsCount.ToString());
            });
        }*/
    }
}
