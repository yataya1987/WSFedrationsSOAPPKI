using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace CoreBackUpCryptoModule.Controllers
{
    public abstract class CoreBackUpCryptoModuleControllerBase: AbpController
    {
        protected CoreBackUpCryptoModuleControllerBase()
        {
            LocalizationSourceName = CoreBackUpCryptoModuleConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
