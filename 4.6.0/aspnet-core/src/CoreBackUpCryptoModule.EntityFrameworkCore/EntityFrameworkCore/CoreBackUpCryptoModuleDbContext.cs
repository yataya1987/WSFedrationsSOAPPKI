using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using CoreBackUpCryptoModule.Authorization.Roles;
using CoreBackUpCryptoModule.Authorization.Users;
using CoreBackUpCryptoModule.MultiTenancy;

namespace CoreBackUpCryptoModule.EntityFrameworkCore
{
    public class CoreBackUpCryptoModuleDbContext : AbpZeroDbContext<Tenant, Role, User, CoreBackUpCryptoModuleDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public CoreBackUpCryptoModuleDbContext(DbContextOptions<CoreBackUpCryptoModuleDbContext> options)
            : base(options)
        {
        }
    }
}
