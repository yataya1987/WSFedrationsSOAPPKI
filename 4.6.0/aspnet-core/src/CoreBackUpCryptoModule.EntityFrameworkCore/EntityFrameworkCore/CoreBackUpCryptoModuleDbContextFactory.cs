using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using CoreBackUpCryptoModule.Configuration;
using CoreBackUpCryptoModule.Web;

namespace CoreBackUpCryptoModule.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class CoreBackUpCryptoModuleDbContextFactory : IDesignTimeDbContextFactory<CoreBackUpCryptoModuleDbContext>
    {
        public CoreBackUpCryptoModuleDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CoreBackUpCryptoModuleDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            CoreBackUpCryptoModuleDbContextConfigurer.Configure(builder, configuration.GetConnectionString(CoreBackUpCryptoModuleConsts.ConnectionStringName));

            return new CoreBackUpCryptoModuleDbContext(builder.Options);
        }
    }
}
