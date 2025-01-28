using AppEspiaoJogo.Common;
using AppEspiaoJogo.Enums;
using AppEspiaoJogo.Game;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace AppEspiaoJogo.Services
{
    public class ServerSocketService
    {
        private bool skipVerification = false;
        private int gameNumber = 0;
        public static Dictionary<TcpClient, bool> ActiveClients = new Dictionary<TcpClient, bool>();

        public ServerSocketService()
        {
            gameNumber = 0;
            NetworkCommon.ServerIsRunning = false;           
        }

        public async Task StartServer()
        {
            try
            {
                _ = RunVerifyClientsAsync(async () =>
                {
                    await VerifyClientsAsync();
                    await Task.CompletedTask;
                }, TimeSpan.FromSeconds(11));

                NetworkCommon.Server = new TcpListener(IPAddress.Any, NetworkCommon.Port);
                NetworkCommon.Server.Start();

                NetworkCommon.ServerIsRunning = true;            
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send<object, (string, string)>(this, "DisplayAlert", ("Info", $"Servidor iniciado na porta {NetworkCommon.Port}\nO IP foi copiado para a área de transferência"));
                    MessagingCenter.Send<object, HostStateEnum>(this, "SetHostState", HostStateEnum.Running);
                });

                while (NetworkCommon.ServerIsRunning)
                {
                    var client = await NetworkCommon.Server.AcceptTcpClientAsync();

                    if (client == null)
                        continue;

                    var clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                    DisconnectClientIfDuplicated(clientIp);

                    NetworkCommon.ConnectedClients.Add(client);

                    SetConnectedClientsCount();

                    skipVerification = true;

                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                if (ex is SocketException)
                {
                    var exT = ex as SocketException;
                    if (exT.SocketErrorCode is SocketError.OperationAborted or SocketError.ConnectionAborted)
                        return;
                }

                NetworkCommon.ServerIsRunning = false;
                gameNumber = 0;
                NetworkCommon.ConnectedClients.Clear();
                ActiveClients.Clear();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send<object, string>(this, "ServerStartError", $"Erro ao iniciar o servidor: {ex.Message}");
                });
            }
        }

        private async Task RunVerifyClientsAsync(Func<Task> action, TimeSpan interval)
        {
            while (true)
            {
                try
                {
                    await action();
                    await Task.Delay(interval);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                }
            }
        }

        private async Task VerifyClientsAsync()
        {
            if (!NetworkCommon.ServerIsRunning)
                return;

            if (skipVerification)
            {
                skipVerification = false;
                return;
            }

            var activeClients = ActiveClients.Where(v => v.Value == true).ToDictionary();

            var clientsAux = new List<TcpClient>();

            foreach (var activeClient in activeClients.Keys)
                clientsAux.Add(activeClient);

            NetworkCommon.ConnectedClients = clientsAux;

            SetConnectedClientsCount();

            foreach (var client in ActiveClients.Keys)
                ActiveClients[client] = false;
        }

        private void SetConnectedClientsCount()
        {
            var connectedClientsCount = NetworkCommon.ConnectedClients.Count();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<object, string>(this, "SetPlayersCount", connectedClientsCount.ToString());
            });
        }

        private void DisconnectClientIfDuplicated(string clientIp)
        {
            var clients = NetworkCommon.ConnectedClients.Where(c => ((IPEndPoint)c.Client.RemoteEndPoint).Address.ToString() == clientIp);

            foreach (var client in clients) 
                client.Close();

            NetworkCommon.ConnectedClients = NetworkCommon.ConnectedClients.Where(c => c.Connected).ToList();
            ActiveClients = NetworkCommon.ConnectedClients.Select(c => new { Client = c, IsActive = true }).ToDictionary(item => item.Client, item => item.IsActive);
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                if (!ActiveClients.Any(c => c.Key == client))
                    ActiveClients.Add(client, true);

                var stream = client.GetStream();
                var buffer = new byte[1024];

                while (NetworkCommon.ServerIsRunning)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        client.Close();
                        RemoveFromConnectedClients(client);
                        break;
                    }

                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (receivedMessage == "DISCONNECT")
                    {
                        if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
                        {
                            client.Close();
                            RemoveFromConnectedClients(client);
                            break;
                        }

                        continue;
                    }

                    if (receivedMessage.Contains("ping"))
                    {
                        if(!ActiveClients.Any(c => c.Key == client))
                            ActiveClients.Add(client, true);
                        else
                            ActiveClients[client] = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    client.Close();
                    RemoveFromConnectedClients(client);
                });
            }
        }

        public void CloseServer()
        {
            gameNumber = 0;
            NetworkCommon.ConnectedClients.Clear();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<object, HostStateEnum>(this, "SetHostState", HostStateEnum.Initial);
            });

            NetworkCommon.ServerIsRunning = false;

            if (NetworkCommon.Server == null)
                return;

            NetworkCommon.StopServer();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<object, (string, string)>(this, "DisplayAlert", ("Info", "Servidor encerrado"));
            });
        }

        public async Task NextGame()
        {
            var newGame = new GameContent
            {
                Number = ++gameNumber,
                Word = Locations.GetRandomLocation()
            };

            var random = new Random();
            var spy = random.Next(NetworkCommon.ConnectedClients.Count() + 1);

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
            var clientSpy = NetworkCommon.ConnectedClients.ElementAt(spy);

            await BroadcastMessageAsync(NetworkCommon.ConnectedClients.Where(c => c!= clientSpy).ToList(), JsonSerializer.Serialize(newGame));

            newGame.Word = "ESPIÃO";

            if (clientSpy.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newGame));
                var stream = clientSpy.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        private async Task GenerateGameForHostBeingTheSpy(GameContent newGame)
        {
            await BroadcastMessageAsync(NetworkCommon.ConnectedClients, JsonSerializer.Serialize(newGame));

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<object, (GameContent, bool)>(this, "SetHostGameData", (newGame, true));
            });
        }

        private bool IsHostSpy(int spy)
        {
            return spy == NetworkCommon.ConnectedClients.Count();
        }

        private async Task BroadcastMessageAsync(List<TcpClient> clients, string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            var failedToSend = new List<TcpClient>();

            foreach (var client in clients)
            {
                try
                {
                    var stream = client.GetStream();

                    if (stream.CanWrite)
                        await stream.WriteAsync(data, 0, data.Length);
                    else
                        failedToSend.Add(client);
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine($"Erro de E/S: {ioEx.Message}");
                    failedToSend.Add(client);
                }
                catch (ObjectDisposedException odEx)
                {
                    Console.WriteLine($"Conexão encerrada: {odEx.Message}");
                    failedToSend.Add(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro desconhecido: {ex.Message}");
                    failedToSend.Add(client);
                }
            }

            RemoveFromConnectedClients(failedToSend);
        }
               
        private void RemoveFromConnectedClients(List<TcpClient> clients)
        {
            NetworkCommon.ConnectedClients.RemoveAll(clients.Contains);
            NetworkCommon.ConnectedClients.RemoveAll(client => !client.Connected);
            ActiveClients = NetworkCommon.ConnectedClients.Select(c => new { Client = c, IsActive = true }).ToDictionary(item => item.Client, item => item.IsActive);
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
            ActiveClients = NetworkCommon.ConnectedClients.Select(c => new { Client = c, IsActive = true }).ToDictionary(item => item.Client, item => item.IsActive);
            var connectedClientsCount = NetworkCommon.ConnectedClients.Count(c => c.Connected);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<object, string>(this, "SetPlayersCount", connectedClientsCount.ToString());
            });
        }
    }
}
