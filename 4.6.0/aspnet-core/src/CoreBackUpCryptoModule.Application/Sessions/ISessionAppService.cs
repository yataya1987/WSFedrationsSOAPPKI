using System.Threading.Tasks;
using Abp.Application.Services;
using CoreBackUpCryptoModule.Sessions.Dto;

namespace CoreBackUpCryptoModule.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
