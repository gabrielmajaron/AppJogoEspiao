using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using AppEspiaoJogo.Common;

namespace AppEspiaoJogo
{
    public static class MauiProgram
    {

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

    #if DEBUG
            builder.Logging.AddDebug();
    #endif
            
            builder.Services.AddSingleton<HubConnection>(provider =>
                new HubConnectionBuilder()
                    .WithUrl("https://127.0.0.1:5001")
                    .Build());

            builder.Services.AddSingleton<HostPage>();

            var app = builder.Build();

            ServiceLocator.ServiceProvider = app.Services;

            return app;
        }
    }
}
