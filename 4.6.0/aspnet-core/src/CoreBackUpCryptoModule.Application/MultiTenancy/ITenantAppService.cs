using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CoreBackUpCryptoModule.MultiTenancy.Dto;

namespace CoreBackUpCryptoModule.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

