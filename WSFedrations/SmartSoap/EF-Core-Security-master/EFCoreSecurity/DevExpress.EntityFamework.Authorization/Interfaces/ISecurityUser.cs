﻿using DevExpress.EntityFramework.SecurityDataStore;
using DevExpress.EntityFramework.SecurityDataStore.Authorization;
using DevExpress.EntityFramework.SecurityDataStore.Security;
using System.Collections.Generic;

namespace DevExpress.EntityFramework.SecurityDataStore.Authorization {
    public interface ISecurityUser : IPermissionsProvider {
        string Name { get; set; }
        string Password { get; set; }
        IEnumerable<IUserRole> UserRoleCollection { get; }
        void AddRole(SecurityRole role);
    }

    public interface IUserRole {
         ISecurityUser User { get; }
         ISecurityRole Role { get; }
    }
}