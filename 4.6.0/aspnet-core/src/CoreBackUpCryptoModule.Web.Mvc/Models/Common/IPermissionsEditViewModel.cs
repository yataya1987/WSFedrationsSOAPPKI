using System.Collections.Generic;
using CoreBackUpCryptoModule.Roles.Dto;

namespace CoreBackUpCryptoModule.Web.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }
    }
}