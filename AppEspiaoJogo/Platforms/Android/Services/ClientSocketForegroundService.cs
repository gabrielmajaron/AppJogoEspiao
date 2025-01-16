/*using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace AppEspiaoJogo.Platforms.Android.Services
{
    [Service(Name = "com.example.AppEspiaoJogo.ClientSocketForegroundService")]
    public class ClientSocketForegroundService : Service
    {
        public const int serviceNotificationId = 1002;

        public override void OnCreate()
        {
            base.OnCreate();

            StartForeground(serviceNotificationId, CreateNotification("Enviando mensagens..."), ForegroundService.TypeRemoteMessaging);
            
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                var clientService = new ClientSocketService();
                Task.Run(() => clientService.ConnectDevice(intent.GetStringExtra("ServerIp")!));
                return StartCommandResult.RedeliverIntent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.InnerException?.Message}");
            }

            return StartCommandResult.RedeliverIntent;
        }

        public override IBinder OnBind(Intent intent) => null;

        private Notification CreateNotification(string message)
        {
            var channelId = "client_foreground_service_channel";
            var channelName = "Foreground Client Service Channel";

            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.Default);
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);

            return new Notification.Builder(this, channelId)
                .SetContentTitle("Jogo em execução")
                .SetContentText(message)
                .Build();
        }
    }
}
*/