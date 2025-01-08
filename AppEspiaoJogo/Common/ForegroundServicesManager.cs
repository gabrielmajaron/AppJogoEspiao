#if ANDROID
using Android.Content;
#endif

namespace AppEspiaoJogo.Common
{
    public static class ForegroundServicesManager
    {
        #if ANDROID
            public static Intent? ActiveServerService;
        #endif
    }
}
