﻿using System;
using System.Linq;
using System.Linq.Expressions;
using DevExpress.EntityFramework.SecurityDataStore.Tests.DbContexts;
using DevExpress.EntityFramework.SecurityDataStore.Tests.Helpers;
using DevExpress.EntityFramework.SecurityDataStore.Security;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DevExpress.EntityFramework.SecurityDataStore.Tests.Security {
    [TestFixture]
    public abstract class EntityRollbackTests {
        [TearDown]
        public void ClearDatabase() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
            }
        }
        [Test]
        public void RollbackDeletedObjectDatabaseValuesDenyPolicy() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> deleteCriteria = (db, obj) => obj.Description == "Good description";
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> writeCriteria = (db, obj) => true;

                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Allow, deleteCriteria);
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Write, OperationState.Allow, writeCriteria);
                dbContextMultiClass.PermissionsContainer.SetTypePermission(typeof(DbContextObject1), SecurityOperation.Read, OperationState.Allow);
                obj1.Description = "Not good description";
                dbContextMultiClass.SaveChanges();

                dbContextMultiClass.Remove(obj1);

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();
                obj1.Description = "Good description";

                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> deleteCriteria = (db, obj) => obj.Description == "Good description";
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> writeCriteria = (db, obj) => true;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Allow, deleteCriteria);
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Write, OperationState.Allow, writeCriteria);
                dbContextMultiClass.PermissionsContainer.SetTypePermission(typeof(DbContextObject1), SecurityOperation.Read, OperationState.Allow);

                dbContextMultiClass.Remove(obj1);

                dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void RollbackMemberDenyPolicy() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Allow, "Description", criteria);
                dbContextMultiClass.PermissionsContainer.SetTypePermission<DbContextObject1>(SecurityOperation.Read, OperationState.Allow);
                obj1.Description = "Good description";

                Assert.AreEqual(EntityState.Modified, dbContextMultiClass.Entry(obj1).State);
                Assert.IsTrue(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                dbContextMultiClass.SaveChanges();

                Assert.AreEqual(EntityState.Unchanged, dbContextMultiClass.Entry(obj1).State);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                obj1.Description = "Not good description";

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                Assert.AreEqual(EntityState.Unchanged, dbContextMultiClass.Entry(obj1).State);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                Assert.AreEqual("Good description", obj1.Description);
            }
        }
        [Test]
        public void RollbackMemberAllowPolicy() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description != "Good description";
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Deny, "Description", criteria);

                obj1.Description = "Good description";

                Assert.AreEqual(EntityState.Modified, dbContextMultiClass.Entry(obj1).State);
                Assert.IsTrue(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                dbContextMultiClass.SaveChanges();

                Assert.AreEqual(EntityState.Unchanged, dbContextMultiClass.Entry(obj1).State);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                obj1.Description = "Not good description";

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                Assert.AreEqual(EntityState.Unchanged, dbContextMultiClass.Entry(obj1).State);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                Assert.AreEqual("Good description", obj1.Description);
            }
        }
        [Test]
        public void RollbackMultipleMembersDenyPolicy() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Allow, "Description", criteria);
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Allow, "DecimalItem", criteria);

                obj1.Description = "Good description";
                obj1.DecimalItem = 10;

                Assert.AreEqual(EntityState.Modified, dbContextMultiClass.Entry(obj1).State);
                Assert.IsTrue(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsTrue(dbContextMultiClass.Entry(obj1).Property("DecimalItem").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                dbContextMultiClass.SaveChanges();

                Assert.AreEqual(EntityState.Unchanged, dbContextMultiClass.Entry(obj1).State);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("DecimalItem").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                obj1.Description = "Not good description";
                obj1.DecimalItem = 20;

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                Assert.AreEqual(EntityState.Unchanged, dbContextMultiClass.Entry(obj1).State);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("DecimalItem").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                Assert.AreEqual("Good description", obj1.Description);
                Assert.AreEqual(10, obj1.DecimalItem);
            }
        }
        [Test]
        public void RollbackMultipleMembersAllowPolicy() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description != "Good description";
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Deny, "Description", criteria);
                // dbContextMultiClass.Security.AddMemberPermission(SecurityOperation.Write, OperationState.Deny, "DecimalItem", criteria);

                obj1.Description = "Good description";
                obj1.DecimalItem = 10;

                Assert.AreEqual(EntityState.Modified, dbContextMultiClass.Entry(obj1).State);
                Assert.IsTrue(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsTrue(dbContextMultiClass.Entry(obj1).Property("DecimalItem").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                dbContextMultiClass.SaveChanges();

                Assert.AreEqual(EntityState.Unchanged, dbContextMultiClass.Entry(obj1).State);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("DecimalItem").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                obj1.Description = "Not good description";
                obj1.DecimalItem = 20;

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                Assert.AreEqual(EntityState.Unchanged, dbContextMultiClass.Entry(obj1).State);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("Description").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("DecimalItem").IsModified);
                Assert.IsFalse(dbContextMultiClass.Entry(obj1).Property("ItemCount").IsModified);

                Assert.AreEqual("Good description", obj1.Description);
                Assert.AreEqual(10, obj1.DecimalItem);
            }
        }
        [Test]
        public void RollbackNavigationProperty() {
            SecurityTestHelper.InitializeContextWithNavigationProperties();
            using(DbContextConnectionClass dbContextConnectionClass = new DbContextConnectionClass()) {
                dbContextConnectionClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);

                dbContextConnectionClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Deny, "Person", SecurityTestHelper.CompanyNameEqualsTwo);

                Assert.AreEqual(3, dbContextConnectionClass.Company.Include(p => p.Person).Count());

                Company company = dbContextConnectionClass.Company.Include(p => p.Person).First(p => p.CompanyName == "2");
                Assert.IsNotNull(company.Person);

                Person newPerson = new Person();
                newPerson.Description = "New person";
                newPerson.Company = company;
                company.Person = newPerson;

                dbContextConnectionClass.Persons.Add(newPerson);

                Assert.AreEqual(EntityState.Modified, dbContextConnectionClass.Entry(company).State);
                Assert.AreEqual(EntityState.Added, dbContextConnectionClass.Entry(newPerson).State);

                SecurityTestHelper.FailSaveChanges(dbContextConnectionClass);

                Assert.AreEqual(EntityState.Unchanged, dbContextConnectionClass.Entry(company).State);

                Assert.IsFalse(dbContextConnectionClass.ChangeTracker.Entries().Any(p => p.Entity == newPerson));

                //Assert.AreEqual(EntityState.Modified, dbContextConnectionClass.Entry(company1).State);
                //Assert.IsTrue(dbContextConnectionClass.Entry(company1).Property("Description").IsModified);
                //Assert.IsTrue(dbContextConnectionClass.Entry(company1).Property("DecimalItem").IsModified);
                //Assert.IsFalse(dbContextConnectionClass.Entry(company1).Property("ItemCount").IsModified);
            }
        }
    }

    [TestFixture]
    public class InMemoryEntityRollbackTests : EntityRollbackTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.IN_MEMORY;
            base.ClearDatabase();
        }
    }

    [TestFixture]
    public class LocalDb2012EntityRollbackTests : EntityRollbackTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.LOCALDB_2012;
            base.ClearDatabase();
        }
    }
}
