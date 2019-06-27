﻿using DevExpress.EntityFramework.SecurityDataStore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.EntityFramework.Authorization {
    public class SecurityDbContextWithUsers : AuthorizationDbContext {
        public DbSet<SecurityUser> Users { get; set; }
        public DbSet<SecurityRole> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<UserRole>().HasOne(p => p.Role).WithMany(p => p.UserRoles).HasForeignKey(p => p.RoleID);
            modelBuilder.Entity<UserRole>().HasOne(p => p.User).WithMany(p => p.UserRoleCollection).HasForeignKey(p => p.UserID);
            modelBuilder.Entity<SecurityPolicyPermission>().HasOne(p => p.SecurityRole).WithMany(p => p.OperationPermissions).HasForeignKey(p => p.SecurityRoleID);
            modelBuilder.Entity<SecurityTypePermission>().HasOne(p => p.SecurityRole).WithMany(p => p.TypePermissions).HasForeignKey(p => p.SecurityRoleID);
            modelBuilder.Entity<SecurityObjectPermission>().HasOne(p => p.SecurityRole).WithMany(p => p.ObjectPermissions).HasForeignKey(p => p.SecurityRoleID);
            modelBuilder.Entity<SecurityMemberPermission>().HasOne(p => p.SecurityRole).WithMany(p => p.MemberPermissions).HasForeignKey(p => p.SecurityRoleID);
        }
        public ISecurityUser GetUserByCredentials(string userName, string password) {
            return this.Users.
                            Include(p => p.UserRoleCollection).ThenInclude(p => p.Role).ThenInclude(p => p.MemberPermissions).
                            Include(p => p.UserRoleCollection).ThenInclude(p => p.Role).ThenInclude(p => p.OperationPermissions).
                            Include(p => p.UserRoleCollection).ThenInclude(p => p.Role).ThenInclude(p => p.ObjectPermissions).
                            Include(p => p.UserRoleCollection).ThenInclude(p => p.Role).ThenInclude(p => p.TypePermissions).
                            FirstOrDefault(p => p.Name == userName && p.Password == password);
        }
        public virtual void Logon(string userName, string password) {
            ISecurityUser currentUser = GetUserByCredentials(userName, password);
            if(currentUser == null) {
                throw new InvalidOperationException("Logon is failed. Try enter right credentials.");
            }
            Logon(currentUser);
        }
    }
}
