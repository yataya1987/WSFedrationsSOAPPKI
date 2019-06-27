using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace CoreBackUpCryptoModule.Localization
{
    public static class CoreBackUpCryptoModuleLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(CoreBackUpCryptoModuleConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(CoreBackUpCryptoModuleLocalizationConfigurer).GetAssembly(),
                        "CoreBackUpCryptoModule.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
