using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CoreBackUpCryptoModule.Configuration;

namespace CoreBackUpCryptoModule.Web.Host.Startup
{
    [DependsOn(
       typeof(CoreBackUpCryptoModuleWebCoreModule))]
    public class CoreBackUpCryptoModuleWebHostModule: AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public CoreBackUpCryptoModuleWebHostModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CoreBackUpCryptoModuleWebHostModule).GetAssembly());
        }
    }
}
