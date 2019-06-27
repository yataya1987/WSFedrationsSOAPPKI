﻿using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using DevExpress.EntityFramework.SecurityDataStore.Tests.Helpers;
using DevExpress.EntityFramework.SecurityDataStore.Tests.DbContexts;

namespace DevExpress.EntityFramework.SecurityDataStore.Tests.Security {
    [TestFixture]
    public abstract class MemberPermissionTests {
        [SetUp]
        public void ClearDatabase() {
            DbContextObject1.Count = 0;
            DbContextMultiClass dbContextMultiClass = new DbContextMultiClass().MakeRealDbContext();
            dbContextMultiClass.ResetDatabase();
        }       
        [Test]
        public void MemberPermissionsAreForbiddenForCreateAndDeleteOperations() {
            foreach(SecurityOperation securityOperation in new SecurityOperation[] { SecurityOperation.Create, SecurityOperation.Delete }) {
                using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";

                    bool withArgumentException = false;
                    try {
                        dbContextMultiClass.PermissionsContainer.AddMemberPermission(securityOperation, OperationState.Allow, "DecimalItem", criteria);
                    }
                    catch(ArgumentException) {
                        withArgumentException = true;
                    }
                    catch(Exception e) {
                        Assert.Fail(e.Message);
                    }
                    Assert.IsTrue(withArgumentException);
                }
            }
        }
        [Test]
        public void ReadObjectAllowPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                DbContextObject1 obj1 = new DbContextObject1();
                obj1.DecimalItem = 10;
                obj1.Description = "Good description";
                DbContextObject1 obj2 = new DbContextObject1();
                obj2.DecimalItem = 20;
                obj2.Description = "Not good description";

                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                Assert.AreEqual(2, dbContextMultiClass.dbContextDbSet1.Count());

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Read, OperationState.Allow, "DecimalItem", criteria);

                Assert.AreEqual(1, dbContextMultiClass.dbContextDbSet1.Count());
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();
                Assert.IsNull(obj1.Description);
                Assert.AreEqual(10, obj1.DecimalItem);
            }
        }
        [Test, Ignore("select doesn't work at the moment")]
        public void ReadMemberAllowPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                DbContextObject1 obj1 = new DbContextObject1();
                obj1.DecimalItem = 10;
                obj1.Description = "Good description";
                DbContextObject1 obj2 = new DbContextObject1();
                obj2.DecimalItem = 20;
                obj2.Description = "Not good description";

                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description != "Good description";
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Read, OperationState.Deny, "DecimalItem", criteria);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> typeCriteria = (db, obj) => true;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, typeCriteria);

                var query = from d in dbContextMultiClass.dbContextDbSet1
                            orderby d.ID
                            select d.DecimalItem;

                Assert.AreEqual(2, query.Count());

                Decimal obj1Decimal = query.First();
                Assert.AreEqual(10, obj1Decimal);

                Decimal obj2Decimal = query.Last();
                Assert.AreEqual(0, obj2Decimal);   // doesn't work now
            }
        }
        [Test]
        public void ReadObjectDenyPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                DbContextObject1 obj1 = new DbContextObject1();
                obj1.DecimalItem = 10;
                obj1.Description = "Good description";
                DbContextObject1 obj2 = new DbContextObject1();
                obj2.DecimalItem = 20;
                obj2.Description = "Not good description";

                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                Assert.AreEqual(2, dbContextMultiClass.dbContextDbSet1.Count());

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.Description == "Not good description";
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Read, OperationState.Deny, "DecimalItem", badCriteria);

                Assert.AreEqual(2, dbContextMultiClass.dbContextDbSet1.Count());
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.First(obj => obj.Description == "Good description");
                Assert.AreEqual("Good description", obj1.Description);
                Assert.AreEqual(10, obj1.DecimalItem);
                DbContextObject1 obj2 = dbContextMultiClass.dbContextDbSet1.First(obj => obj.Description == "Not good description");
                Assert.AreEqual("Not good description", obj2.Description);
                Assert.AreEqual(0, obj2.DecimalItem);
            }
        }
        [Test]
        public void ReadObjectMultiplePermissions() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                DbContextObject1 obj1 = new DbContextObject1();
                obj1.ItemCount = 5;
                obj1.DecimalItem = 10;
                DbContextObject1 obj2 = new DbContextObject1();
                obj2.ItemCount = 8;
                obj2.DecimalItem = 20;
                DbContextObject1 obj3 = new DbContextObject1();
                obj3.ItemCount = 10;
                obj3.DecimalItem = 30;

                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.SaveChanges();
                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.SaveChanges();
                dbContextMultiClass.Add(obj3);
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                Assert.AreEqual(dbContextMultiClass.dbContextDbSet1.Count(), 3);

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria = (db, obj) => obj.ItemCount > 3;
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Read, OperationState.Allow, "DecimalItem", goodCriteria);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria2 = (db, obj) => obj.ItemCount < 9;
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Read, OperationState.Allow, "DecimalItem", goodCriteria2);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.ItemCount == 8;
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Read, OperationState.Deny, "DecimalItem", badCriteria);

                IEnumerable<DbContextObject1> objects = dbContextMultiClass.dbContextDbSet1.OrderBy(p => p.ID).AsEnumerable();
                Assert.AreEqual(2, objects.Count());

                DbContextObject1 obj1 = objects.ElementAt(0);
                Assert.AreEqual(0, obj1.ItemCount);
                Assert.AreEqual(10, obj1.DecimalItem);

                DbContextObject1 obj2 = objects.ElementAt(1);
                Assert.AreEqual(0, obj2.ItemCount);
                Assert.AreEqual(30, obj2.DecimalItem);
            }
        }
        [Test]
        public void WriteMemberAllowPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Allow, "Description", criteria);

                obj1.Description = "Good description";
                dbContextMultiClass.SaveChanges();

                obj1.Description = "Not good description";
                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.Description = "Good description";
                dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void WriteMemberDenyPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.Description == "Not good description";
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Deny, "DecimalItem", badCriteria);

                obj1.Description = "Good description";
                obj1.DecimalItem = 20;

                dbContextMultiClass.SaveChanges();

                obj1.Description = "Not good description";
                obj1.DecimalItem = 10;

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.Description = "Good description";
                obj1.DecimalItem = 10;
                dbContextMultiClass.SaveChanges();
            }
        }
        /*
        [Test]
        public void WriteMemberAndCriteriaWithDatabaseValueDenyPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.Database.EnsureCreated();
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.DecimalItem > db.DatabaseEntity(obj).DecimalItem;
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Deny, "DecimalItem", badCriteria);

                obj1.DecimalItem = 20;
                dbContextMultiClass.SaveChanges();

                obj1.DecimalItem = 10;

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.DecimalItem = 22;
                dbContextMultiClass.SaveChanges();
            }
        }
        */
        [Test]
        public void WriteMembersMultiplePermissions() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria = (db, obj) => obj.ItemCount > 3;
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Allow, "ItemCount", goodCriteria);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria2 = (db, obj) => obj.ItemCount < 9;
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Allow, "ItemCount", goodCriteria2);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.ItemCount == 8;
                dbContextMultiClass.PermissionsContainer.AddMemberPermission(SecurityOperation.Write, OperationState.Deny, "ItemCount", badCriteria);              

                obj1.ItemCount = 8;
                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);   
                Assert.AreEqual(0, obj1.DecimalItem);

                obj1.ItemCount = 6;
                dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void IsGrantedAllowPermission() {
            foreach(SecurityOperation securityOperation in new SecurityOperation[] { SecurityOperation.Read, SecurityOperation.Write }) {
                using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                    DbContextObject1 obj1 = new DbContextObject1();
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));

                    dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                    dbContextMultiClass.PermissionsContainer.AddMemberPermission(securityOperation, OperationState.Allow, "DecimalItem", criteria);

                    obj1.Description = "Not good description";
                    Assert.IsFalse(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1, "DecimalItem"));

                    obj1.Description = "Good description";
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1, "DecimalItem"));
                }
            }
        }
        [Test]
        public void IsGrantedDenyPermission() {
            foreach(SecurityOperation securityOperation in new SecurityOperation[] { SecurityOperation.Read, SecurityOperation.Write }) {
                using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                    DbContextObject1 obj1 = new DbContextObject1();
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));

                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.Description == "Not good description";
                    dbContextMultiClass.PermissionsContainer.AddMemberPermission(securityOperation, OperationState.Deny, "DecimalItem", badCriteria);

                    obj1.Description = "Not good description";
                    Assert.IsFalse(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1, "DecimalItem"));

                    obj1.Description = "Good description";
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1, "DecimalItem"));
                }
            }
        }
        [Test]
        public void IsGrantedMultiplePermissions() {
            foreach(SecurityOperation securityOperation in new SecurityOperation[] { SecurityOperation.Read, SecurityOperation.Write }) {
                using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                    DbContextObject1 obj1 = new DbContextObject1();
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), SecurityOperation.Write, obj1, null));

                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria = (db, obj) => obj.ItemCount > 3;
                    dbContextMultiClass.PermissionsContainer.AddMemberPermission(securityOperation, OperationState.Allow, "DecimalItem", goodCriteria);

                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria2 = (db, obj) => obj.ItemCount < 9;
                    dbContextMultiClass.PermissionsContainer.AddMemberPermission(securityOperation, OperationState.Allow, "DecimalItem", goodCriteria2);

                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.ItemCount == 8;
                    dbContextMultiClass.PermissionsContainer.AddMemberPermission(securityOperation, OperationState.Deny, "DecimalItem", badCriteria);

                    obj1.ItemCount = 8;
                    Assert.IsFalse(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1, "DecimalItem"));

                    obj1.ItemCount = 5;
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1, "DecimalItem"));
                }
            }
        }
        [Test]
        public void IsGrantedObjectAndMember() {
            using(DbContextMultiClass dbContext = new DbContextMultiClass()) {
                DbContextObject1 dbContextObject1_1 = new DbContextObject1();
                dbContextObject1_1.ItemName = "1";
                dbContext.Add(dbContextObject1_1);
                dbContext.SaveChanges();

                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                dbContext.PermissionsContainer.AddObjectPermission<DbContextMultiClass, DbContextObject1>(
                    SecurityOperation.Read,
                    OperationState.Deny,
                    (p, d) => d.ItemName == "1");

                Assert.AreEqual(dbContext.dbContextDbSet1.Count(), 0);
            }
        }
        [Test]
        public void IsGrantedDenyPermissionPolicyObjectDenyMemberAllow() {
            using(DbContextMultiClass dbContext = new DbContextMultiClass()) {
                DbContextObject1 dbContextObject1_1 = new DbContextObject1();
                dbContextObject1_1.ItemName = "1";
                dbContext.Add(dbContextObject1_1);
                dbContext.SaveChanges();

                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                dbContext.PermissionsContainer.AddObjectPermission<DbContextMultiClass, DbContextObject1>(
                   SecurityOperation.Read,
                   OperationState.Allow,
                   (p, d) => d.ItemName == "1");
                dbContext.PermissionsContainer.AddObjectPermission<DbContextMultiClass, DbContextObject1>(
                    SecurityOperation.Read,
                    OperationState.Deny,
                    (p, d) => d.ItemName == "1");
                dbContext.PermissionsContainer.AddMemberPermission<DbContextMultiClass, DbContextObject1>(SecurityOperation.Read, OperationState.Allow, "ItemName", (p, b) => b.ItemName == "1");
                Assert.AreEqual(dbContext.dbContextDbSet1.Count(), 1);
            }
        }
    }

    [TestFixture]
    public class InMemoryMemberPermissionTests : MemberPermissionTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.IN_MEMORY;
            base.ClearDatabase();
        }
    }

    [TestFixture]
    public class LocalDb2012MemberPermissionTests : MemberPermissionTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.LOCALDB_2012;
            base.ClearDatabase();
        }
    }
}
