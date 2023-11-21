using ApiElTiempo.DataManagement;
using ApiElTiempo.Logic.Interface;

namespace ApiElTiempo.Extensions
{
    public static class StartupTrasient
    {
        public static void configureTrasient(this IServiceCollection services)
        {
            services.AddTransient<IUser, User>();
        }
    }
}
