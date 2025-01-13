using AppEspiaoJogo.Game;
using System.Text.Json;
using System.Text;
using System.Net.Sockets;
using AppEspiaoJogo.Enums;
using AppEspiaoJogo.Common;
using System.Runtime.InteropServices;

#pragma warning disable CS0618

namespace AppEspiaoJogo.Services
{

    public class ClientSocketService
    {
        public async Task ConnectDevice(string serverIp)
        {
            try
            {
                NetworkCommon.Client = new TcpClient();

                await NetworkCommon.Client.ConnectAsync(serverIp, NetworkCommon.Port);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send<object, ClientStateEnum>(this, "SetClientState", ClientStateEnum.Running);
                });

                _ = ListenToServerAsync();
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send<object, string>(this, "ConnectionError", $"Erro ao conectar ao servidor: {ex.Message}");
                });
            }
        }

        private async Task ListenToServerAsync()
        {
            try
            {
                var stream = NetworkCommon.Client.GetStream();
                var buffer = new byte[1024];

                while (NetworkCommon.IsClientConnected())
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            MessagingCenter.Send<object, (string, string)>(this, "DisplayAlert", ("Aviso", "Desconectado"));
                            MessagingCenter.Send<object, ClientStateEnum>(this, "SetClientState", ClientStateEnum.Initial);
                        });
                        break;
                    }

                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var gameContent = GetGameContentFromMessage(ref receivedMessage);

                    if (gameContent == null)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            MessagingCenter.Send<object, (string, string)>(this, "DisplayAlert", ("Erro", $"Erro ao obter mensagem: {receivedMessage}"));
                        });
                        continue;
                    }

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MessagingCenter.Send<object, GameContent>(this, "SetClientGameData", gameContent!);
                    });
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SocketException socketEx)
                {
                    if (socketEx.SocketErrorCode != SocketError.OperationAborted)
                        await HandleErrorAsync(ex.Message);
                }
                else
                    await HandleErrorAsync(ex.Message);
            }
            finally
            {
                NetworkCommon.Client.Close();
                NetworkCommon.Client.Dispose();
            }
        }

        private async Task HandleErrorAsync(string errorMessage)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send<object, (string, string)>(this, "DisplayAlert", ("Erro", errorMessage));
                MessagingCenter.Send<object, ClientStateEnum>(this, "SetClientState", ClientStateEnum.Initial);
            });
        }

        private GameContent? GetGameContentFromMessage(ref string receivedMessage)
        {
            GameContent? gameContent = null;
            List<GameContent>? gameContentList = null;

            try
            {
                gameContent = JsonSerializer.Deserialize<GameContent>(receivedMessage);
            }
            catch (Exception _)
            {
            }

            if (gameContent != null)
                return gameContent;

            receivedMessage = receivedMessage.Replace("}", "},");
            receivedMessage = receivedMessage.Remove(receivedMessage.Length - 1);
            receivedMessage = $"[{receivedMessage}]";

            try
            {
                gameContentList = JsonSerializer.Deserialize<List<GameContent>?>(receivedMessage);
            }
            catch (Exception _)
            {
            }

            if (gameContentList != null)
                return gameContentList.Last();

            return null;
        }
    }
}
