using MVCElTiempo.DataManagement;
using MVCElTiempo.DataManagement.Interface;
using MVCElTiempo.Infraestructure;

namespace MVCElTiempo.Extensions
{
    public static class InterfaceAggregation
    {
        public static void ConfigureTransient(this IServiceCollection services)
        {
            services.AddTransient<TenantInfo>();
            services.AddTransient<IUser, User>();
        }
    }
}
