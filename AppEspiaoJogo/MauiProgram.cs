using FFImageLoading.Maui;
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
                })
                .UseFFImageLoading();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<Image, Microsoft.Maui.Handlers.ImageHandler>();
            });

            return builder.Build();
        }
    }
}
