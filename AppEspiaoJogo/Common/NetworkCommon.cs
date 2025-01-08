using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace AppEspiaoJogo.Common
{
    public static class NetworkCommon
    {
        public static readonly int Port = 5000;

        public static bool ServerIsRunning = false;
        public static TcpListener Server;
        public static List<TcpClient> ConnectedClients;

        public static TcpClient Client;

        public static string GetLocalIpAddress()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProperties = networkInterface.GetIPProperties();

                    foreach (var ipAddress in ipProperties.UnicastAddresses)
                    {
                        if (ipAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ipAddress.Address.ToString();
                        }
                    }
                }
            }

            return "127.0.0.1";
        }

        public static int ConnectedClientsCount()
        {
            if (ConnectedClients == null)
                return 0;

            return ConnectedClients.Count();
        }

        public static bool IsClientConnected() => Client?.Connected == true;

        public static void StopServer()
        {
            if (Server != null)
            {
                foreach (var client in ConnectedClients)
                {
                    if (client.Connected)
                    {
                        client.Close();
                    }
                }
                Server.Stop();
            }
            ServerIsRunning = false;
        }
    }
}
