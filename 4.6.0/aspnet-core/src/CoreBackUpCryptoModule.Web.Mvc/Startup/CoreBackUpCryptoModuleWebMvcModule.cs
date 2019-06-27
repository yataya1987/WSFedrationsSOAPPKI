using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CoreBackUpCryptoModule.Configuration;

namespace CoreBackUpCryptoModule.Web.Startup
{
    [DependsOn(typeof(CoreBackUpCryptoModuleWebCoreModule))]
    public class CoreBackUpCryptoModuleWebMvcModule : AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public CoreBackUpCryptoModuleWebMvcModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.Navigation.Providers.Add<CoreBackUpCryptoModuleNavigationProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CoreBackUpCryptoModuleWebMvcModule).GetAssembly());
        }
    }
}
