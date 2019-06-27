using System.Threading.Tasks;
using CoreBackUpCryptoModule.Configuration.Dto;

namespace CoreBackUpCryptoModule.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
