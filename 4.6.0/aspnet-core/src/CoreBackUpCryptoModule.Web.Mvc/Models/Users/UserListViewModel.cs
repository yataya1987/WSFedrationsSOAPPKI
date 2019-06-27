using System.Collections.Generic;
using CoreBackUpCryptoModule.Roles.Dto;
using CoreBackUpCryptoModule.Users.Dto;

namespace CoreBackUpCryptoModule.Web.Models.Users
{
    public class UserListViewModel
    {
        public IReadOnlyList<UserDto> Users { get; set; }

        public IReadOnlyList<RoleDto> Roles { get; set; }
    }
}
