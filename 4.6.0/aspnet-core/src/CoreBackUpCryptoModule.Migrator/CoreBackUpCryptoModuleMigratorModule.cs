using Microsoft.Extensions.Configuration;
using Castle.MicroKernel.Registration;
using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CoreBackUpCryptoModule.Configuration;
using CoreBackUpCryptoModule.EntityFrameworkCore;
using CoreBackUpCryptoModule.Migrator.DependencyInjection;

namespace CoreBackUpCryptoModule.Migrator
{
    [DependsOn(typeof(CoreBackUpCryptoModuleEntityFrameworkModule))]
    public class CoreBackUpCryptoModuleMigratorModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;

        public CoreBackUpCryptoModuleMigratorModule(CoreBackUpCryptoModuleEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

            _appConfiguration = AppConfigurations.Get(
                typeof(CoreBackUpCryptoModuleMigratorModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                CoreBackUpCryptoModuleConsts.ConnectionStringName
            );

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.ReplaceService(
                typeof(IEventBus), 
                () => IocManager.IocContainer.Register(
                    Component.For<IEventBus>().Instance(NullEventBus.Instance)
                )
            );
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CoreBackUpCryptoModuleMigratorModule).GetAssembly());
            ServiceCollectionRegistrar.Register(IocManager);
        }
    }
}
