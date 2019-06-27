using Abp.Application.Services.Dto;

namespace CoreBackUpCryptoModule.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

