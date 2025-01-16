/*using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AppEspiaoJogo.Services;

namespace AppEspiaoJogo.Platforms.Android.Services
{
    [Service(Name = "com.example.AppEspiaoJogo.ServerSocketForegroundService")]
    public class ServerSocketForegroundService : Service
    {
        public const int serviceNotificationId = 1001;

        public override void OnCreate()
        {
            base.OnCreate();

            if (Build.VERSION.SdkInt >= (BuildVersionCodes)34) // API 34+
                StartForeground(serviceNotificationId, CreateNotification("Enviando mensagens..."), ForegroundService.TypeRemoteMessaging);
            else
                StartForeground(serviceNotificationId, CreateNotification("Enviando mensagens..."));
        }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                var serverService = new ServerSocketService();
                Task.Run(() => serverService.StartServer());
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
            const string channelId = "server_foreground_service_channel";
            var channelName = "Foreground Server Service Channel";

            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High);
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