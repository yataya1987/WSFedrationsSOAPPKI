using Abp.AutoMapper;
using CoreBackUpCryptoModule.Sessions.Dto;

namespace CoreBackUpCryptoModule.Web.Views.Shared.Components.TenantChange
{
    [AutoMapFrom(typeof(GetCurrentLoginInformationsOutput))]
    public class TenantChangeViewModel
    {
        public TenantLoginInfoDto Tenant { get; set; }
    }
}
