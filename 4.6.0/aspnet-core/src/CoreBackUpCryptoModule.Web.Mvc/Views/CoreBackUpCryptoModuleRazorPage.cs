using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;

namespace CoreBackUpCryptoModule.Web.Views
{
    public abstract class CoreBackUpCryptoModuleRazorPage<TModel> : AbpRazorPage<TModel>
    {
        [RazorInject]
        public IAbpSession AbpSession { get; set; }

        protected CoreBackUpCryptoModuleRazorPage()
        {
            LocalizationSourceName = CoreBackUpCryptoModuleConsts.LocalizationSourceName;
        }
    }
}
