using AppEspiaoJogo.Common;
using AppEspiaoJogo.Services;
using Microsoft.Extensions.Logging;

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


            builder.Services.AddSingleton<HostPage>();
            builder.Services.AddSingleton<LocationsPage>();
            builder.Services.AddSingleton<ClientSocketService>();
            builder.Services.AddSingleton<ServerSocketService>();

            var app = builder.Build();

            ServiceLocator.ServiceProvider = app.Services;

            return app;
        }
    }
}
