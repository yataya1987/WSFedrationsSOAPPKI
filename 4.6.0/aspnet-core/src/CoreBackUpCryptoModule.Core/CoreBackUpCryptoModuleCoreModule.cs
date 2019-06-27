using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using CoreBackUpCryptoModule.Authorization.Roles;
using CoreBackUpCryptoModule.Authorization.Users;
using CoreBackUpCryptoModule.Configuration;
using CoreBackUpCryptoModule.Localization;
using CoreBackUpCryptoModule.MultiTenancy;
using CoreBackUpCryptoModule.Timing;

namespace CoreBackUpCryptoModule
{
    [DependsOn(typeof(AbpZeroCoreModule))]
    public class CoreBackUpCryptoModuleCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            // Declare entity types
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

            CoreBackUpCryptoModuleLocalizationConfigurer.Configure(Configuration.Localization);

            // Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = CoreBackUpCryptoModuleConsts.MultiTenancyEnabled;

            // Configure roles
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            Configuration.Settings.Providers.Add<AppSettingProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CoreBackUpCryptoModuleCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }
    }
}
