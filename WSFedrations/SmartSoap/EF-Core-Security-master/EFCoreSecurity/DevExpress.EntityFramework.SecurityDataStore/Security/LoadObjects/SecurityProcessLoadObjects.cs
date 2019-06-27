﻿using DevExpress.EntityFramework.SecurityDataStore.Security.LoadObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.EntityFramework.SecurityDataStore.Security {
    public class SecurityProcessLoadObjects : ISecurityProcessLoadObjects {
        private BaseSecurityDbContext securityDbContext;
        private ISecurityObjectRepository securityObjectRepository;
        private IPermissionProcessor permissionProcessor;
        private ISecurityInformationFiller fillSecurityObjects;
        public SecurityProcessLoadObjects(DbContext securityDbContext, ISecurityObjectRepository securityObjectRepository, IPermissionProcessor permissionProcessor) {
            this.securityDbContext = (BaseSecurityDbContext)securityDbContext;         
            this.securityObjectRepository = securityObjectRepository;
            this.permissionProcessor = permissionProcessor;
            fillSecurityObjects = new FillSecurityObjects(permissionProcessor, this.securityDbContext.Model);
        }
        public IEnumerable<object> ProcessObjects(IEnumerable<object> objects) {
            IEnumerable<object> allObjects = securityDbContext.Model.GetAllLinkedObjects(objects);
            IEnumerable<SecurityObjectBuilder> duplicateBuilders = securityObjectRepository.GetDuplicateBuilders(allObjects);
            securityObjectRepository.RemoveBuilders(duplicateBuilders);
            securityDbContext.ChangeTracker.TryStopObjectsInChangeTracker(duplicateBuilders.Select(p => p.SecurityObject));
            IEnumerable<object> notEntityObjects = GetNotEntityObjects(objects);
            IEnumerable<object> blockedObjects = GetBlockedObjects(allObjects);
            IEnumerable<object> processingEntities = CreateProcessingEntities(allObjects, blockedObjects);            
            IEnumerable<SecurityObjectBuilder> modyficationsObjects = ModificationsMembersHelper.GetModificationsDifferences(permissionProcessor, securityDbContext.RealDbContext.Model, processingEntities, blockedObjects);
            securityObjectRepository.RegisterBuilders(modyficationsObjects);
            IEnumerable<object> securityObjects = CreateSecurityObjects(processingEntities, blockedObjects, modyficationsObjects);            
            IEnumerable<object> resultObject = GetOrCreateResultObjects(securityObjects, objects, modyficationsObjects);
            fillSecurityObjects.FillSecurityInformation(modyficationsObjects);
            return resultObject;
        }
        private IEnumerable<object> CreateProcessingEntities(IEnumerable<object> allObjects, IEnumerable<object> denyObjects) {
            List<object> processingEntities = new List<object>();
            foreach(object targetObject in allObjects) {
                if(!denyObjects.Contains(targetObject)) {
                    processingEntities.Add(targetObject);
                }
            }
            return processingEntities;
        }
        private IEnumerable<object> GetOrCreateResultObjects(IEnumerable<object> securityObjects, IEnumerable<object> objects, IEnumerable<SecurityObjectBuilder> modyficationsObjects) {
            List<object> resultObject = new List<object>();

            foreach(object targetObject in objects) {
                SecurityObjectBuilder objectMetaData = modyficationsObjects.FirstOrDefault(p => p.RealObject == targetObject);
                if(objectMetaData != null && objectMetaData.SecurityObject != null) {
                    resultObject.Add(objectMetaData.SecurityObject);
                }
                else {
                    resultObject.Add(targetObject);
                }
            }
            return resultObject;
        }
        private IEnumerable<object> CreateSecurityObjects(IEnumerable<object> processingEntity, IEnumerable<object> denyObjects, IEnumerable<SecurityObjectBuilder> modyficationsObjects) {
            List<object> securityObjects = new List<object>();
            foreach(object targetObject in processingEntity) {
                SecurityObjectBuilder modifyObjectMetaInfo = modyficationsObjects.First(p => p.RealObject == targetObject);
                if(modifyObjectMetaInfo != null) {
                    object securityObject;
                    if(modifyObjectMetaInfo.SecurityObject == null) {
                        securityObject = modifyObjectMetaInfo.CreateSecurityObject(securityDbContext.Model, securityObjectRepository);
                    }
                    else {
                        securityObject = modifyObjectMetaInfo.SecurityObject;
                    }
                    securityObjects.Add(securityObject);
                }
            }
            return securityObjects;
        }       
        private List<string> GetDenyProperties(object targetObject) {
            List<string> denyMembers = new List<string>();
            Type targetType = targetObject.GetType();
            IEntityType entityType = securityDbContext.RealDbContext.Model.FindEntityType(targetType);
            IEnumerable<INavigation> properties = entityType.GetNavigations();
            IEnumerable<PropertyInfo> propertiesInfo = targetType.GetRuntimeProperties();
            foreach(IProperty property in properties) {
                PropertyInfo propertyInfo = propertiesInfo.FirstOrDefault(p => p.Name == property.Name);
                if(property.IsKey()) {
                    continue;
                }
                if(property.GetContainingForeignKeys().Count() > 0) {
                    continue;
                }
                if(property.GetContainingKeys().Count() > 0) {
                    continue;
                }
                if(propertyInfo != null && propertyInfo.GetGetMethod().IsStatic) {
                    continue;
                }
                bool isGranted = permissionProcessor.IsGranted(targetType, SecurityOperation.Read, targetObject, property.Name);
                if(!isGranted) {
                    denyMembers.Add(property.Name);
                }
            }
            return denyMembers;
        }
        private IEnumerable<object> GetBlockedObjects(IEnumerable<object> allObject) {
            List<object> objectsToDelete = new List<object>();
            foreach(object targetObject in allObject) {
                bool result = permissionProcessor.IsGranted(targetObject.GetType(), SecurityOperation.Read, targetObject);
                if(!result) {
                    objectsToDelete.Add(targetObject);
                }
            }
            return objectsToDelete;
        }
        private IEnumerable<object> GetNotEntityObjects(IEnumerable<object> objects) {
            List<object> notEntityObjects = new List<object>();
            foreach(object targetObject in objects) {
                IEntityType entityType = securityDbContext.Model.FindEntityType(targetObject.GetType());
                if(entityType == null) {
                    notEntityObjects.Add(targetObject);
                }
            }
            return notEntityObjects;
        }  
    }
}
