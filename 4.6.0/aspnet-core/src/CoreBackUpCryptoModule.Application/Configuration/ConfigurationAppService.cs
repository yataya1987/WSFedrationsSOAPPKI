using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using CoreBackUpCryptoModule.Configuration.Dto;

namespace CoreBackUpCryptoModule.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : CoreBackUpCryptoModuleAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
