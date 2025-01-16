/*#if ANDROID
using Android.Content;
using AppEspiaoJogo.Platforms.Android.Services;

namespace AppEspiaoJogo.Common
{
    public static class ForegroundServicesManager
    {
        private static Context _context = Android.App.Application.Context;
        public static Intent? ClientService = new Intent(_context, typeof(ClientSocketForegroundService));
        public static Intent? ServerService = new Intent(_context, typeof(ServerSocketForegroundService));

        public static void StartClient(string serverIp)
        {
            ClientService!.PutExtra("ServerIp", serverIp);

            _context!.StartForegroundService(ClientService!);
        }

        public static void StopClient()
        {
            _context.StopService(ClientService);
        }

        public static void StartServer()
        {
            _context!.StartForegroundService(ServerService!);
        }

        public static void StopServer()
        {
            _context.StopService(ServerService);

            if (NetworkCommon.ServerIsRunning)
                NetworkCommon.StopServer();
        }
    }
}
#endif
*/