using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCElTiempo.Infraestructure
{
    public class TenantInfoMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantInfoMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var tenantInfo = context.RequestServices.GetRequiredService<TenantInfo>();
            var tenantName = context.Request.Headers["Client"];

            if (string.IsNullOrEmpty(tenantName))
            {
                tenantInfo.Name = "DATASYSTEM";
            }
            else
            {
                tenantInfo.Name = tenantName;
            }

            await _next(context);
        }
    }
}
