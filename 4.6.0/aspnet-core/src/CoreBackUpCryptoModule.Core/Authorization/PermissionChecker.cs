using Abp.Authorization;
using CoreBackUpCryptoModule.Authorization.Roles;
using CoreBackUpCryptoModule.Authorization.Users;

namespace CoreBackUpCryptoModule.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
