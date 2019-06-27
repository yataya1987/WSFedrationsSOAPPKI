﻿using DevExpress.EntityFramework.SecurityDataStore.Security;
using DevExpress.EntityFramework.SecurityDataStore.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.EntityFramework.SecurityDataStore {
    public class SecurityStrategy : ISecurityStrategy {
        protected BaseSecurityDbContext securityDbContext;
        public IPermissionProcessor PermissionProcessor {
            get {
                return securityDbContext.GetService<IPermissionProcessor>();
            }
        }
        public ISecurityExpressionBuilder SecurityExpressionBuilder {
            get {
                return securityDbContext.GetService<ISecurityExpressionBuilder>();
            }
        }
        public ISecurityObjectRepository SecurityObjectRepository {
            get {
                return securityDbContext.GetService<ISecurityObjectRepository>();
            }
        }
        public ISecurityProcessLoadObjects SecurityProcessLoadObjects {
            get {
                return securityDbContext.GetService<ISecurityProcessLoadObjects>();
            }
        }
        public ISecuritySaveObjects SecuritySaveObjects {
            get {
                return securityDbContext.GetService<ISecuritySaveObjects>();
            }
        }
        public IPermissionsProvider PermissionsProvider {
            get {
                return securityDbContext.GetService<IPermissionsProvider>();
            }
        }
        public virtual bool IsGranted(Type type, SecurityOperation operation) {
            return PermissionProcessor.IsGranted(type, operation, null);
        }
        public virtual bool IsGranted(Type type, SecurityOperation operation, object targetObject) {
            return PermissionProcessor.IsGranted(type, operation, targetObject, null);
        }
        public virtual bool IsGranted(Type type, SecurityOperation operation, object targetObject, string memberName) {
            return PermissionProcessor.IsGranted(type, operation, targetObject, memberName);
        }
        public SecurityStrategy(DbContext dbContext) {
            securityDbContext = (BaseSecurityDbContext)dbContext;
        }
    }
}
