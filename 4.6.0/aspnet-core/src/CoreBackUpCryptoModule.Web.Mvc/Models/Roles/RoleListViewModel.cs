using System.Collections.Generic;
using CoreBackUpCryptoModule.Roles.Dto;

namespace CoreBackUpCryptoModule.Web.Models.Roles
{
    public class RoleListViewModel
    {
        public IReadOnlyList<RoleListDto> Roles { get; set; }

        public IReadOnlyList<PermissionDto> Permissions { get; set; }
    }
}
