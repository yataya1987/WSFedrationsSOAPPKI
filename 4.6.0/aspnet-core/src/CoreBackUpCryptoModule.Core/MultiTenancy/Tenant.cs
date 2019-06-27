using Abp.MultiTenancy;
using CoreBackUpCryptoModule.Authorization.Users;

namespace CoreBackUpCryptoModule.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
