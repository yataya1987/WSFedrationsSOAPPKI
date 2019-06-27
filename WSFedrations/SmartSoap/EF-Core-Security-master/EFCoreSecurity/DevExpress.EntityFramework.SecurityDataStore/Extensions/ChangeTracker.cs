﻿using DevExpress.EntityFramework.SecurityDataStore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace DevExpress.EntityFramework.SecurityDataStore {
    public static class ChangeTrackerExtensions {
        public static IStateManager GetStateManager(this ChangeTracker changeTracker) {
            return ((IInfrastructure<IStateManager>)changeTracker).Instance;
        }
        public static EntityEntry GetEntity(this ChangeTracker changeTracker, object targetObject) {
            return changeTracker.Entries().FirstOrDefault(p => p.Entity == targetObject);
        }
        public static InternalEntityEntry GetPrincipalEntityEntryCurrentValue(this ChangeTracker changeTracker, InternalEntityEntry targetEntity, IForeignKey foreignKey) {
            IEnumerable<InternalEntityEntry> targetEntities = changeTracker.GetStateManager().Entries.Where(p => Equals(p.EntityType.ClrType, foreignKey.PrincipalEntityType.ClrType));
            // IEnumerable<InternalEntityEntry> targetEntities = changeTracker.Entries().Where(p => p.  Equals(p.EntityType.ClrType, foreignKey.PrincipalEntityType.ClrType));
            InternalEntityEntry principalEntityEntry = null;
            foreach(InternalEntityEntry entityEntry in targetEntities) {
                bool result = true;
                for(int i = 0; i < foreignKey.Properties.Count; i++) {
                    object foreignValue = targetEntity.GetCurrentValue(foreignKey.Properties[i]);
                    object principalValue = entityEntry.GetCurrentValue(foreignKey.PrincipalKey.Properties[i]);

                    if (!Equals(foreignValue, principalValue)) {
                        result = false;
                        break;
                    }
                }
                if(result) {
                    principalEntityEntry = entityEntry;
                }
            }
            return principalEntityEntry;
        }
        public static InternalEntityEntry GetPrincipaEntityEntryOriginalValue(this ChangeTracker changeTracker, InternalEntityEntry targetEntity, IForeignKey foreignKey) {
            IEnumerable<InternalEntityEntry> targetEntities = changeTracker.GetStateManager().Entries.Where(p => Equals(p.EntityType.ClrType, foreignKey.PrincipalEntityType.ClrType));
            InternalEntityEntry principalEntityEntry = null;
            foreach(InternalEntityEntry entityEntry in targetEntities) {
                bool result = true;
                for(int i = 0; i < foreignKey.Properties.Count; i++) {
                    object foreignValue = targetEntity.GetOriginalValue(foreignKey.Properties[i]);
                    object principalValue = entityEntry.GetOriginalValue(foreignKey.PrincipalKey.Properties[i]);

                    if (!Equals(foreignValue, principalValue)) {
                        result = false;
                        break;
                    }
                }
                if(result) {
                    principalEntityEntry = entityEntry;
                }
            }
            return principalEntityEntry;
        }
        public static IEnumerable<ModifiedObjectMetadata> GetModifiedObjectMetadata(this ChangeTracker changeTracker) {
            IEnumerable<InternalEntityEntry> entities = changeTracker.GetStateManager().Entries.Where(p => p.EntityState == EntityState.Modified);
            return GetModifiedObjectMetadata(entities, changeTracker);
        }
        public static IEnumerable<ModifiedObjectMetadata> GetModifiedObjectMetadaForAddedObjects(this ChangeTracker changeTracker) {
            IEnumerable<InternalEntityEntry> entities = changeTracker.GetStateManager().Entries.Where(p => p.EntityState == EntityState.Added);
            return GetModifiedObjectMetadata(entities, changeTracker);
        }
        public static void TryStopObjectsInChangeTracker(this ChangeTracker changeTracker, IEnumerable<object> targetObjects) {
            foreach(object obj in targetObjects) {
                TryStopObjectInChangeTracker(changeTracker, obj);
            }        
        }
        public static bool TryStopObjectInChangeTracker(this ChangeTracker changeTracker, object targetObject) {
            bool result;
            IStateManager infrastructure = changeTracker.GetInfrastructure();
            InternalEntityEntry entity = infrastructure.Entries.FirstOrDefault(p => p.Entity == targetObject);
            if(entity != null) {
                infrastructure.StopTracking(entity);
                result = true;
            }
            else {
                result = false;
            }
            return result;
        }
        private static IEnumerable<ModifiedObjectMetadata> GetModifiedObjectMetadata(IEnumerable<InternalEntityEntry> entitiesEntry, ChangeTracker changeTracker) {
            List<ModifiedObjectMetadata> modifyObjectsMetada = new List<ModifiedObjectMetadata>();
            foreach(InternalEntityEntry entityEntry in entitiesEntry) {
                switch(entityEntry.EntityState) {

                    case EntityState.Modified:
                        ProcessModifiedEntity(modifyObjectsMetada, entityEntry, changeTracker);
                        break;
                    case EntityState.Added:
                        ProcessAddedEntity(modifyObjectsMetada, entityEntry, changeTracker);
                        break;
                }

            }
            return modifyObjectsMetada;
        }
        private static void ProcessAddedEntity(List<ModifiedObjectMetadata> modifiedObjectsMetadataList, InternalEntityEntry entityEntry, ChangeTracker changeTracker) {
            IEnumerable<IForeignKey> foreignKeys = entityEntry.EntityType.GetForeignKeys();
            // ModifiedObjectMetadata modifyObjectMetada = GetOrCreateMetaData(modifiedObjectsMetadataList, entityEntry.Entity);
            IEnumerable<PropertyEntry> properties = entityEntry.GetProperties();
            foreach(IForeignKey foreignKey in foreignKeys) {
                for(int i = 0; i < foreignKey.Properties.Count(); i++) {
                    PropertyEntry propertyEntry = properties.First(p => p.Metadata.Name == foreignKey.Properties[0].Name);
                    if(propertyEntry.CurrentValue != null && propertyEntry.CurrentValue.Equals(null)) {
                        continue;
                    }
                    InternalEntityEntry principaEntityEntryCurrentValue = changeTracker.GetPrincipalEntityEntryCurrentValue(entityEntry, foreignKey);
                    if(principaEntityEntryCurrentValue != null && principaEntityEntryCurrentValue.EntityState != EntityState.Added) {
                        ProcessPrincipalEntity(entityEntry, modifiedObjectsMetadataList, foreignKey, principaEntityEntryCurrentValue);
                    }
                }
            }
        }
        private static void ProcessModifiedEntity(List<ModifiedObjectMetadata> modifyObjectsMetada, InternalEntityEntry entityEntry, ChangeTracker changeTracker) {
            ModifiedObjectMetadata modifyObjectMetada = GetOrCreateMetaData(modifyObjectsMetada, entityEntry.Entity);
            IEnumerable<PropertyEntry> properties = entityEntry.GetProperties();
            ProcessProperties(entityEntry.Entity, modifyObjectMetada, properties);
            ProcessNavigations(entityEntry, modifyObjectsMetada, modifyObjectMetada, properties, changeTracker);
        }
        private static void ProcessNavigations(InternalEntityEntry entityEntry, List<ModifiedObjectMetadata> modifyObjectsMetada, ModifiedObjectMetadata modifyObjectMetada, IEnumerable<PropertyEntry> properties, ChangeTracker changeTracker) {
            IEnumerable<IForeignKey> foreignKeys = entityEntry.EntityType.GetForeignKeys();
            foreach(IForeignKey foreignKey in foreignKeys) {
                for(int i = 0; i < foreignKey.Properties.Count(); i++) {
                    PropertyEntry propertyEntry = properties.First(p => p.Metadata.Name == foreignKey.Properties[0].Name);
                    if(!propertyEntry.IsModified && entityEntry.EntityState != EntityState.Added) {
                        continue;
                    }
                    modifyObjectMetada.ForeignKeys.Add(propertyEntry.Metadata.Name, propertyEntry.CurrentValue);
                    IEnumerable<INavigation> findNavigationsToForeign = foreignKey.FindNavigationsFrom(entityEntry.EntityType);
                    foreach(INavigation nav in findNavigationsToForeign) {
                        modifyObjectMetada.NavigationProperties.Add(nav.Name);
                    }

                    InternalEntityEntry principaEntityEntryOriginalValue = changeTracker.GetPrincipaEntityEntryOriginalValue(entityEntry, foreignKey);
                    if(principaEntityEntryOriginalValue != null) {
                        ProcessPrincipalEntity(entityEntry, modifyObjectsMetada, foreignKey, principaEntityEntryOriginalValue);
                    }
                    InternalEntityEntry principaEntityEntryCurrentValue = changeTracker.GetPrincipalEntityEntryCurrentValue(entityEntry, foreignKey);
                    if(principaEntityEntryCurrentValue != null) {
                        ProcessPrincipalEntity(entityEntry, modifyObjectsMetada, foreignKey, principaEntityEntryCurrentValue);
                    }
                }
            }
        }
        private static void ProcessPrincipalEntity(InternalEntityEntry entityEntry, List<ModifiedObjectMetadata> modifyObjectsMetada, IForeignKey foreignKey, InternalEntityEntry principaEntityEntry) {
            ModifiedObjectMetadata modifyObjectMetadaNavigation = GetOrCreateMetaData(modifyObjectsMetada, principaEntityEntry.Entity);
            IEnumerable<INavigation> findNavigationsTo = foreignKey.FindNavigationsFrom(principaEntityEntry.EntityType);
            foreach(var entityNavigation in findNavigationsTo) {
                if(!modifyObjectMetadaNavigation.NavigationProperties.Contains(entityNavigation.Name)) {
                    modifyObjectMetadaNavigation.NavigationProperties.Add(entityNavigation.Name);

                }
            }
        }
        private static void ProcessProperties(object targetObject, ModifiedObjectMetadata modifiedObjectMetadata, IEnumerable<PropertyEntry> properties) {
            foreach(PropertyEntry propertyEntry in properties) {
                if(propertyEntry.IsModified) {
                    if(propertyEntry.Metadata.IsKeyOrForeignKey()) {
                        continue;
                    }
                    modifiedObjectMetadata.Properties.Add(propertyEntry.Metadata.Name, propertyEntry.CurrentValue);
                }
            }
        }
        private static ModifiedObjectMetadata GetOrCreateMetaData(List<ModifiedObjectMetadata> modifiedObjectMetadataList, object targetObject) {
            ModifiedObjectMetadata modifiedObjectMetadata = modifiedObjectMetadataList.FirstOrDefault(p => Equals(p.Object, targetObject));
            if(modifiedObjectMetadata == null) {
                modifiedObjectMetadata = new ModifiedObjectMetadata(targetObject);
                modifiedObjectMetadataList.Add(modifiedObjectMetadata);
            }
            return modifiedObjectMetadata;
        }
    }
}
