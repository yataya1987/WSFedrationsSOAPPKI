using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace CoreBackUpCryptoModule.EntityFrameworkCore
{
    public static class CoreBackUpCryptoModuleDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<CoreBackUpCryptoModuleDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<CoreBackUpCryptoModuleDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
