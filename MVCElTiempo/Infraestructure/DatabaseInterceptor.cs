using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCElTiempo.Infraestructure
{
    public class DatabaseInterceptor
    {
        private readonly TenantInfo tenantInfo;
        public DatabaseInterceptor(TenantInfo tenantInfo)
        {
            this.tenantInfo = tenantInfo;
        }
    }
}
