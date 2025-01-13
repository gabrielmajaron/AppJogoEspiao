using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AppEspiaoJogo.Services;

namespace AppEspiaoJogo.Platforms.Android.Services
{
    [Service(Name = "com.example.AppEspiaoJogo.ClientSocketForegroundService")]
    public class ClientSocketForegroundService : Service
    {
        public const int ServiceNotificationId = 1002;
        const string channelId = "foreground_service_channel";

        public override void OnCreate()
        {
            base.OnCreate();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, "Remote Messaging Channel", NotificationImportance.High);
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }

            if (Build.VERSION.SdkInt >= (BuildVersionCodes)34) // API 34+
            {
                StartForeground(ServiceNotificationId, CreateNotification("Enviando mensagens..."), ForegroundService.TypeRemoteMessaging);
            }
            else
            {
                StartForeground(ServiceNotificationId, CreateNotification("Enviando mensagens..."));
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                var clientService = new ClientSocketService();
                Task.Run(() => clientService.ConnectDevice(intent.GetStringExtra("ServerIp")!, true));
                return StartCommandResult.Sticky;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.InnerException?.Message}");
            }

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent) => null;

        private Notification CreateNotification(string message)
        {
            var channelId = "foreground_service_channel";
            var channelName = "Foreground Service Channel";
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, channelName, NotificationImportance.Default);
                notificationManager.CreateNotificationChannel(channel);
            }

            return new Notification.Builder(this, channelId)
                .SetContentTitle("Jogo em execução")
                .SetContentText(message)
                .Build();
        }
    }
}
