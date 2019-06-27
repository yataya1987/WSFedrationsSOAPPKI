using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CoreBackUpCryptoModule.Authorization;

namespace CoreBackUpCryptoModule
{
    [DependsOn(
        typeof(CoreBackUpCryptoModuleCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class CoreBackUpCryptoModuleApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<CoreBackUpCryptoModuleAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(CoreBackUpCryptoModuleApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddProfiles(thisAssembly)
            );
        }
    }
}
