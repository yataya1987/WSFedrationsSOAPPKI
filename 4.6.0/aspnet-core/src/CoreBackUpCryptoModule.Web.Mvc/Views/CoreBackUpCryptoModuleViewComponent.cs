using Abp.AspNetCore.Mvc.ViewComponents;

namespace CoreBackUpCryptoModule.Web.Views
{
    public abstract class CoreBackUpCryptoModuleViewComponent : AbpViewComponent
    {
        protected CoreBackUpCryptoModuleViewComponent()
        {
            LocalizationSourceName = CoreBackUpCryptoModuleConsts.LocalizationSourceName;
        }
    }
}
