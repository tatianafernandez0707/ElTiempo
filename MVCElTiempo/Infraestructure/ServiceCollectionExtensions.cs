using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MVCElTiempo.Models.ContextEntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCElTiempo.Infraestructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseConnectionPerTenant(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient((serviceProvider) =>
            {
                var tenant = serviceProvider.GetRequiredService<TenantInfo>();
                var connectionString = configuration.GetConnectionString("DATASYSTEM"); //Aqui puede ir cualquier conexion 
                var options = new DbContextOptionsBuilder<MvcContext>()
                    .UseSqlServer(connectionString)
                    .Options;
                var context = new MvcContext(options);
                return context;
            });

            return services;
        }
    }
}
