namespace AppEspiaoJogo.Common
{
    public static class ServiceLocator
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static T GetService<T>()
        {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}