﻿using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Collections.Generic;
using DevExpress.EntityFramework.SecurityDataStore.Tests.Helpers;
using DevExpress.EntityFramework.SecurityDataStore.Tests.DbContexts;

namespace DevExpress.EntityFramework.SecurityDataStore.Tests.Security {
    [TestFixture]
    public abstract class ObjectPermissionTests {
        [SetUp]
        public void ClearDatabase() {
            DbContextObject1.Count = 0;
            // DbContextMultiClass dbContextMultiClass = new DbContextMultiClass().MakeRealDbContext();
            // dbContextMultiClass.ResetDatabase();
        }
        [Test]
        public void ReadObjectAllowPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                
                DbContextObject1 obj1 = new DbContextObject1();
                obj1.Description = "Good description";
                DbContextObject1 obj2 = new DbContextObject1();
                obj2.Description = "Not good description";

                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                Assert.AreEqual(2, dbContextMultiClass.dbContextDbSet1.Count());

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, criteria);
                
                Assert.AreEqual(1, dbContextMultiClass.dbContextDbSet1.Count());
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();
                Assert.AreEqual("Good description", obj1.Description);
            }
        }
        [Test]
        public void ReadObjectAllowPermissionClearPermissions() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();

                DbContextObject1 obj1 = new DbContextObject1();
                obj1.Description = "Good description";
                DbContextObject1 obj2 = new DbContextObject1();
                obj2.Description = "Not good description";

                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                Assert.AreEqual(2, dbContextMultiClass.dbContextDbSet1.Count());

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, criteria);
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                Assert.AreEqual(2, dbContextMultiClass.dbContextDbSet1.Count());
            }
        }   
        [Test]
        public void AnotherDbContextInNewSecurity() {
            // SecurityDbContext originalDbContext;
            // ISecurityStrategy originalSecurityStrategy;
            var securityDbContextFI = typeof(SecurityStrategy).GetRuntimeFields().First(p => p.Name == "securityDbContext");
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();

                DbContextObject1 obj1 = new DbContextObject1();
                obj1.Description = "aaa";

                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.SaveChanges();
                if(dbContextMultiClass.Security is SecurityStrategy) {
                    BaseSecurityDbContext value = (BaseSecurityDbContext)securityDbContextFI.GetValue(dbContextMultiClass.Security);
                    Assert.AreEqual(dbContextMultiClass.RealDbContext, value.RealDbContext);
                    // originalDbContext = dbContextMultiClass.realDbContext;
                    // originalSecurityStrategy = dbContextMultiClass.Security;
                }
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                DbContextObject1 obj2 = new DbContextObject1();
                obj2.Description = "bbb";

                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.SaveChanges();

                // Assert.AreNotEqual(originalSecurityStrategy, dbContextMultiClass.Security);
                // Assert.AreNotEqual(originalDbContext, dbContextMultiClass.Security.GetDbContext());
                DbContext securityDbContext = dbContextMultiClass.RealDbContext;
                if(dbContextMultiClass.Security is SecurityStrategy) {
                    SecurityDbContext value = (SecurityDbContext)securityDbContextFI.GetValue(dbContextMultiClass.Security);
                    Assert.AreEqual(securityDbContext, value.RealDbContext);
                }
            }
        }
        [Test]
        public void ReadObjectAllowPermissionComplexCriteria() {
            // SecurityDbContext originalDbContext;
            // ISecurityStrategy originalSecurityStrategy;
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();

                DbContextObject1 obj11 = new DbContextObject1();
                obj11.Description = "aaa";
                DbContextObject1 obj12 = new DbContextObject1();
                obj12.Description = "bbb";

                DbContextObject2 obj2 = new DbContextObject2();
                obj2.Description = "a";

                dbContextMultiClass.Add(obj11);
                dbContextMultiClass.Add(obj12);
                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.SaveChanges();

                // originalDbContext = dbContextMultiClass.realDbContext;
                // originalSecurityStrategy = dbContextMultiClass.Security;
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                // Assert.AreEqual(2, dbContextMultiClass.dbContextDbSet1.Count());

                // Assert.AreNotEqual(originalSecurityStrategy, dbContextMultiClass.Security);
                // Assert.AreEqual(dbContextMultiClass.Security, dbContextMultiClass.Security.GetDbContext());

                // Assert.AreNotEqual(originalDbContext, dbContextMultiClass.Security.GetDbContext());
                // Assert.AreEqual(dbContextMultiClass.realDbContext, dbContextMultiClass.Security.GetDbContext());

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description.Contains(db.dbContextDbSet2.First().Description);
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, criteria);



                Assert.AreEqual(1, dbContextMultiClass.dbContextDbSet1.Count());
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();
                Assert.AreEqual("aaa", obj1.Description);
            }
        }        
        [Test]
        public void ReadObjectDenyPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();

                DbContextObject1 obj1 = new DbContextObject1();
                obj1.Description = "Good description";
                DbContextObject1 obj2 = new DbContextObject1();
                obj2.Description = "Not good description";

                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                Assert.AreEqual(dbContextMultiClass.dbContextDbSet1.Count(), 2);

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Not good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, criteria);

                Assert.AreEqual(1, dbContextMultiClass.dbContextDbSet1.Count());
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();
                Assert.AreEqual("Good description", obj1.Description);
            }
        }
        [Test]
        public void ReadObjectMultiplePermissions() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();

                DbContextObject1 obj1 = new DbContextObject1();
                obj1.DecimalItem = 5;
                DbContextObject1 obj2 = new DbContextObject1();
                obj2.DecimalItem = 8;
                DbContextObject1 obj3 = new DbContextObject1();
                obj3.DecimalItem = 10;

                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.Add(obj2);
                dbContextMultiClass.Add(obj3);
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                var o = dbContextMultiClass.dbContextDbSet1.ToArray();
                Assert.AreEqual(dbContextMultiClass.dbContextDbSet1.Count(), 3);

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria = (db, obj) => obj.DecimalItem > 3;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, goodCriteria);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria2 = (db, obj) => obj.DecimalItem < 9;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, goodCriteria2);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.DecimalItem == 8;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, badCriteria);

                var to = dbContextMultiClass.dbContextDbSet1.ToArray();

                Assert.AreEqual(2, dbContextMultiClass.dbContextDbSet1.Count());              
            }
        }
        [Test]
        public void ReadObjectCriteriaByCompanyNameWhenNavigationPropertyIsNull() {
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                dbContext.ResetDatabase();

                Company company1 = new Company() { CompanyName = "DevExpress" };
                Company company2 = new Company() { CompanyName = "Microsoft" };
                //Person person1 = new Person() { PersonName = "John", One = company1 };
                //Person person2 = new Person() { PersonName = "Jack", One = null };
                //Person person3 = new Person() { PersonName = "Jack", One = company2 };

                Office office1 = new Office() { Name = "London", Company = company1 };
                Office office2 = new Office() { Name = "Paris", Company = null };
                Office office3 = new Office() { Name = "Rome", Company = company2 };

                dbContext.Add(office1);
                dbContext.Add(office2);
                dbContext.Add(office3);

                dbContext.SaveChanges();
            }
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                IQueryable<Office> offices = dbContext.Offices;
                Assert.AreEqual(3, offices.Count());
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextConnectionClass, Office, bool>> criteria = (db, obj) => obj.Company != null && obj.Company.CompanyName == "DevExpress";
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, criteria);

                IQueryable<Office> securedOffices = dbContext.Offices.Include(o => o.Company);
                Assert.AreEqual(2, securedOffices.Count());
                Assert.IsTrue(securedOffices.Any(p=>p.Name == "Rome"));
                Assert.IsTrue(securedOffices.Any(p => p.Name == "Paris"));
            }
        }
        [Test]
        public void ReadObjectCriteriaByNavigationPropertyIsNotNull() {
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                dbContext.ResetDatabase();

                Company company1 = new Company() { CompanyName = "DevExpress" };
                Company company2 = new Company() { CompanyName = "Microsoft" };

                //Person person1 = new Person() { PersonName = "John", One = company1 };
                //Person person2 = new Person() { PersonName = "Bruce", One = null };
                //Person person3 = new Person() { PersonName = "Jack", One = company2 };

                Office office1 = new Office() { Name = "London", Company = company1 };
                Office office2 = new Office() { Name = "Paris", Company = null };
                Office office3 = new Office() { Name = "Rome", Company = company2 };

                dbContext.Add(office1);
                dbContext.Add(office2);
                dbContext.Add(office3);

                dbContext.SaveChanges();
            }
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                IQueryable<Office> offices = dbContext.Offices.Include(o => o.Company);
                Assert.AreEqual(offices.Count(), 3);
                foreach(Office office in offices) {
                    if(office.Name != "Paris") {
                        Assert.IsNotNull(office.Company); 
                    }
                    else {
                        Assert.IsNull(office.Company);
                    }
                }
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextConnectionClass, Office, bool>> criteria = (db, obj) => obj.Company == null;
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, criteria);

                int nativeCriteriaCount = dbContext.GetRealDbContext().Offices.Include(p => p.Company).Where(obj => obj.Company != null).Count();

                IQueryable<Office> securedOffices = dbContext.Offices.Include(p => p.Company);
                Assert.AreEqual(nativeCriteriaCount, securedOffices.Count()); // ef doesn't support null check in db             
            }
        }
        [Test]
        public void ReadObjectCriteriaNavigationPropertyIsNotNull() {
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                dbContext.ResetDatabase();

                Company company1 = new Company() { CompanyName = "DevExpress" };
                Company company2 = new Company() { CompanyName = "Microsoft" };
                //Person person1 = new Person() { PersonName = "John", One = company1 };
                //Person person2 = new Person() { PersonName = "James", One = company2 };
                Office office1 = new Office() { Name = "London", Company = company1 };
                Office office2 = new Office() { Name = "Paris", Company = company2 };

                dbContext.Add(office1);
                dbContext.Add(office2);
                dbContext.SaveChanges();
            }
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                IQueryable<Office> offices = dbContext.Offices;
                Assert.AreEqual(2, offices.Count());
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextConnectionClass, Office, bool>> criteria = (db, obj) => obj.Company.CompanyName == "DevExpress";
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, criteria);

                IQueryable<Office> securedOffices = dbContext.Offices.Include(o => o.Company);
                Assert.AreEqual(1, securedOffices.Count());
                Assert.AreEqual("Paris", securedOffices.First().Name);
            }
        }
        [Test]
        public void ReadObjectAnyCriteriaNavigationCollectionIsEmpty() {
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                dbContext.ResetDatabase();

                Company company1 = new Company() { CompanyName = "DevExpress" };
                Company company2 = new Company() { CompanyName = "Microsoft" };
                //Person person1 = new Person() { PersonName = "John", One = company1 };
                //Person person2 = new Person() { PersonName = "Jack", One = null };

                Office office1 = new Office() { Name = "London", Company = company1 };
                Office office2 = new Office() { Name = "Paris", Company = null };

                dbContext.Add(office1);
                dbContext.Add(office2);
                dbContext.Add(company2);
                dbContext.SaveChanges();
            }
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                IQueryable<Company> companies = dbContext.Company;
                Assert.AreEqual(companies.Count(), 2);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextConnectionClass, Company, bool>> criteria = (db, obj) => obj.Offices.Any(o => o.Name == "London");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, criteria);

                IQueryable<Company> securedCompanies = dbContext.Company.Include(p => p.Offices);
                Assert.AreEqual(1, securedCompanies.Count());
                Assert.AreEqual("Microsoft", securedCompanies.First().CompanyName);
            }
        }
        [Test]
        public void ReadObjectAnyCriteriaNavigationCollectionIsNotEmpty() {
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                dbContext.ResetDatabase();

                Company company1 = new Company() { CompanyName = "DevExpress" };
                Company company2 = new Company() { CompanyName = "Microsoft" };
                //Person person1 = new Person() { PersonName = "John", One = company1 };
                //Person person2 = new Person() { PersonName = "Jack", One = company2 };

                Office office1 = new Office() { Name = "London", Company = company1 };
                Office office2 = new Office() { Name = "Paris", Company = company2 };

                dbContext.Add(office1);
                dbContext.Add(office2);
                dbContext.SaveChanges();
            }
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                IQueryable<Company> companies = dbContext.Company;
                Assert.AreEqual(companies.Count(), 2);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextConnectionClass, Company, bool>> criteria = (db, obj) => obj.Offices.Any(p => p.Name == "London");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, criteria);

                IQueryable<Company> securedCompanies = dbContext.Company.Include(p => p.Offices);
                Assert.AreEqual(securedCompanies.Count(), 1);
                Assert.AreEqual(securedCompanies.First().CompanyName, "Microsoft");
            }
        }
        [Test]
        public void ReadObjectContainsCriteriaNavigationCollectionIsEmpty() {
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                dbContext.ResetDatabase();

                Company company1 = new Company() { CompanyName = "DevExpress" };
                Company company2 = new Company() { CompanyName = "Microsoft" };
                //Person person1 = new Person() { PersonName = "John", One = company1 };
                //Person person2 = new Person() { PersonName = "Jack", One = null };
                Office office1 = new Office() { Name = "London", Company = company1 };
                Office office2 = new Office() { Name = "Paris", Company = null };

                dbContext.Add(office1);
                dbContext.Add(office2);
                dbContext.Add(company2);
                dbContext.SaveChanges();
            }
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                IQueryable<Company> companies = dbContext.Company;
                Assert.AreEqual(companies.Count(), 2);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextConnectionClass, Company, bool>> criteria = (db, obj) => obj.Offices.Contains(db.Offices.First(p => p.Name == "London"));
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, criteria);

                IQueryable<Company> securedCompanies = dbContext.Company.Include(p => p.Offices);
                Assert.AreEqual(securedCompanies.Count(), 1);
                Assert.AreEqual(securedCompanies.First().CompanyName, "Microsoft");
            }
        }
        [Test]
        public void ReadObjectContainsCriteriaNavigationCollectionIsNotEmpty() {
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                dbContext.ResetDatabase();

                Company company1 = new Company() { CompanyName = "DevExpress" };
                Company company2 = new Company() { CompanyName = "Microsoft" };
                //Person person1 = new Person() { PersonName = "John", One = company1 };
                //Person person2 = new Person() { PersonName = "Jack", One = company2 };
                Office office1 = new Office() { Name = "London", Company = company1 };
                Office office2 = new Office() { Name = "Paris", Company = company2 };

                dbContext.Add(office1);
                dbContext.Add(office2);
                dbContext.SaveChanges();
            }
            using(DbContextConnectionClass dbContext = new DbContextConnectionClass()) {
                IQueryable<Company> companies = dbContext.Company;
                Assert.AreEqual(companies.Count(), 2);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextConnectionClass, Company, bool>> criteria = (db, obj) => obj.Offices.Contains(db.Offices.First(o => o.Name == "London"));
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, criteria);

                IQueryable<Company> securedCompanies = dbContext.Company.Include(p => p.Offices);
                Assert.AreEqual(securedCompanies.Count(), 1);
                Assert.AreEqual(securedCompanies.First().CompanyName, "Microsoft");
            }
        }
        [Test]
        public void ReadObjectContactDenyByTask() {
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                dbContext.ResetDatabase();
                SecurityTestHelper.CreateITDepartment(dbContext);
                dbContext.SaveChanges();
            }
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                IQueryable<Contact> contacts = dbContext.Contacts;
                Assert.AreEqual(contacts.Count(), 3);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextManyToManyRelationship, Contact, bool>> criteria = (db, obj) => obj.ContactTasks.Any(p => p.Task.Description == "Draw");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, criteria);

                IQueryable<Contact> securedContacts = dbContext.Contacts.Include(p => p.Department).Include(c => c.ContactTasks).ThenInclude(ct => ct.Task);
                Assert.AreEqual(securedContacts.Count(), 2);
            }
        }
        [Test]
        public void ReadObjectContactAllowByTask() {
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                dbContext.ResetDatabase();
                SecurityTestHelper.CreateITDepartment(dbContext);
                dbContext.SaveChanges();
            }
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                IQueryable<Contact> contacts = dbContext.Contacts;
                Assert.AreEqual(contacts.Count(), 3);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextManyToManyRelationship, Contact, bool>> criteria = (db, obj) => obj.ContactTasks.Any(p => p.Task.Description == "Draw");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, criteria);

                IQueryable<Contact> securedContacts = dbContext.Contacts.Include(p => p.Department).Include(c => c.ContactTasks).ThenInclude(ct => ct.Task);
                Assert.AreEqual(securedContacts.Count(), 1);
            }
        }
        [Test]
        public void ReadObject_ContactDenyByDepartment_TaskDenyByProhibitedContact() {
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                dbContext.ResetDatabase();
                SecurityTestHelper.InitializeData(dbContext);
            }
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                IQueryable<Contact> contacts = dbContext.Contacts;
                Assert.AreEqual(contacts.Count(), 9);
                IQueryable<DemoTask> tasks = dbContext.Tasks;
                Assert.AreEqual(tasks.Count(), 9);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextManyToManyRelationship, Contact, bool>> contactCriteria = (db, obj) => obj.Department != null && obj.Department.Title == "IT";
                Expression<Func<DbContextManyToManyRelationship, DemoTask, bool>> taskCriteria = (db, obj) => obj.ContactTasks.Any(p => p.Contact.Name == "John");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, contactCriteria);
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, taskCriteria);

                int nativeContactCriteriaCount = dbContext.GetRealDbContext().Contacts.Where(obj => !(obj.Department != null && obj.Department.Title == "IT")).Count();

                IQueryable<Contact> securedContacts = dbContext.Contacts.Include(p => p.Department).Include(c => c.ContactTasks).ThenInclude(ct => ct.Task);
                Assert.AreEqual(nativeContactCriteriaCount, securedContacts.Count());

                int nativeDemoTaskCriteriaCount = dbContext.GetRealDbContext().Tasks.Where(obj => !(obj.ContactTasks.Any(p => p.Contact.Name == "John"))).Count();

                IQueryable<DemoTask> securedTasks = dbContext.Tasks.Include(c => c.ContactTasks).ThenInclude(ct => ct.Contact);
                Assert.AreEqual(nativeDemoTaskCriteriaCount, securedTasks.Count());
            }
        }
        [Test]
        public void ReadObject_ContactDenyByDepartment_TaskDenyByPermittedContact() {
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                dbContext.ResetDatabase();
                SecurityTestHelper.InitializeData(dbContext);
            }
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                IQueryable<Contact> contacts = dbContext.Contacts;
                Assert.AreEqual(contacts.Count(), 9);
                IQueryable<DemoTask> tasks = dbContext.Tasks;
                Assert.AreEqual(tasks.Count(), 9);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextManyToManyRelationship, Contact, bool>> contactCriteria = (db, obj) => obj.Department != null && obj.Department.Title == "IT";
                Expression<Func<DbContextManyToManyRelationship, DemoTask, bool>> taskCriteria = (db, obj) => obj.ContactTasks.Any(p => p.Contact.Name == "Zack");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, contactCriteria);
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, taskCriteria);

                int nativeContactCriteriaCount = dbContext.GetRealDbContext().Contacts.Where(obj => !(obj.Department != null && obj.Department.Title == "IT")).Count();

                IQueryable<Contact> securedContacts = dbContext.Contacts.Include(p => p.Department).Include(c => c.ContactTasks).ThenInclude(ct => ct.Task);
                Assert.AreEqual(nativeContactCriteriaCount, securedContacts.Count());

                int nativeDemoTaskCriteriaCount = dbContext.GetRealDbContext().Tasks.Where(obj => !(obj.ContactTasks.Any(p => p.Contact.Name == "Zack"))).Count();

                IQueryable<DemoTask> securedTasks = dbContext.Tasks.Include(c => c.ContactTasks).ThenInclude(ct => ct.Contact);
                Assert.AreEqual(nativeDemoTaskCriteriaCount, securedTasks.Count());
            }
        }
        [Test]
        public void ReadObject_ContactAllowByDepartment_TaskAllowByProhibitedContact() {
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                dbContext.ResetDatabase();
                SecurityTestHelper.InitializeData(dbContext);
            }
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                IQueryable<Contact> contacts = dbContext.Contacts;
                Assert.AreEqual(contacts.Count(), 9);
                IQueryable<DemoTask> tasks = dbContext.Tasks;
                Assert.AreEqual(tasks.Count(), 9);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextManyToManyRelationship, Contact, bool>> contactCriteria = (db, obj) => obj.Department != null && obj.Department.Title == "IT";
                Expression<Func<DbContextManyToManyRelationship, DemoTask, bool>> taskCriteria = (db, obj) => obj.ContactTasks.Any(p => p.Contact.Name == "Zack");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, contactCriteria);
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, taskCriteria);

                IQueryable<Contact> securedContacts = dbContext.Contacts.Include(p => p.Department).Include(c => c.ContactTasks).ThenInclude(ct => ct.Task);
                Assert.AreEqual(securedContacts.Count(), 2);

                IQueryable<DemoTask> securedTasks = dbContext.Tasks.Include(c => c.ContactTasks).ThenInclude(ct => ct.Contact);
                Assert.AreEqual(securedTasks.Count(), 1);
            }
        }
        [Test]
        public void ReadObject_ContactAllowByDepartment_TaskAllowByPermittedContact() {
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                dbContext.ResetDatabase();
                SecurityTestHelper.InitializeData(dbContext);
            }
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                IQueryable<Contact> contacts = dbContext.Contacts;
                Assert.AreEqual(contacts.Count(), 9);
                IQueryable<DemoTask> tasks = dbContext.Tasks;
                Assert.AreEqual(tasks.Count(), 9);
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextManyToManyRelationship, Contact, bool>> contactCriteria = (db, obj) => obj.Department != null && obj.Department.Title == "IT";
                Expression<Func<DbContextManyToManyRelationship, DemoTask, bool>> taskCriteria = (db, obj) => obj.ContactTasks.Any(p => p.Contact.Name == "John");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, contactCriteria);
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, taskCriteria);

                IQueryable<Contact> securedContacts = dbContext.Contacts.Include(p => p.Department).Include(c => c.ContactTasks).ThenInclude(ct => ct.Task);
                Assert.AreEqual(securedContacts.Count(), 2);

                IQueryable<DemoTask> securedTasks = dbContext.Tasks.Include(c => c.ContactTasks).ThenInclude(ct => ct.Contact);
                Assert.AreEqual(securedTasks.Count(), 1);
            }
        }
        [Test]
        public void ReadObject_ContactDenyByDepartment_TaskDenyByContactDepartment() {
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                dbContext.ResetDatabase();
                SecurityTestHelper.InitializeData(dbContext);
            }
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                IQueryable<Contact> contacts = dbContext.Contacts.Include(p=>p.Department);
                Assert.AreEqual(9, contacts.Count());
                IQueryable<DemoTask> tasks = dbContext.Tasks;
                Assert.AreEqual(9, tasks.Count());
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);
                Expression<Func<DbContextManyToManyRelationship, Contact, bool>> contactCriteria = (db, obj) => obj.Department != null && obj.Department.Title == "IT";
                Expression<Func<DbContextManyToManyRelationship, DemoTask, bool>> taskCriteria = (db, obj) => obj.ContactTasks.Any(p => p.Contact.Department != null && p.Contact.Department.Title == "IT");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, contactCriteria);
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Deny, taskCriteria);

                int nativeCountContact = dbContext.GetRealDbContext().Contacts.Where(p => !(p.Department != null && p.Department.Title == "IT")).Count();

                IQueryable<Contact> securedContacts = dbContext.Contacts;
                Assert.AreEqual(nativeCountContact, securedContacts.Count());

                int nativeCountDemoTask = dbContext.GetRealDbContext().Tasks.Where(obj => !(obj.ContactTasks.Any(p => p.Contact.Department != null && p.Contact.Department.Title == "IT"))).Count();

                IQueryable <DemoTask> securedTasks = dbContext.Tasks;
                DemoTask securedTask = securedTasks.First();
                Assert.AreEqual(nativeCountDemoTask, securedTasks.Count());
            }
        }
        [Test]
        public void ReadObject_ContactAllowByDepartment_TaskAllowByContactDepartment() {
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                dbContext.ResetDatabase();
                SecurityTestHelper.InitializeData(dbContext);
            }
            using(DbContextManyToManyRelationship dbContext = new DbContextManyToManyRelationship()) {
                IQueryable<Contact> contacts = dbContext.Contacts;
                Assert.AreEqual(9, contacts.Count());
                IQueryable<DemoTask> tasks = dbContext.Tasks;
                Assert.AreEqual(9, tasks.Count());
                dbContext.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);
                Expression<Func<DbContextManyToManyRelationship, Contact, bool>> contactCriteria = (db, obj) => obj.Department != null && obj.Department.Title == "IT";
                Expression<Func<DbContextManyToManyRelationship, DemoTask, bool>> taskCriteria = (db, obj) => obj.ContactTasks.Any(p => p.Contact.Department != null && p.Contact.Department.Title == "IT");
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, contactCriteria);
                dbContext.PermissionsContainer.AddObjectPermission(SecurityOperation.Read, OperationState.Allow, taskCriteria);

                IQueryable<Contact> securedContacts = dbContext.Contacts.Include(p => p.Department).Include(c => c.ContactTasks).ThenInclude(ct => ct.Task);
                Contact securedContact = securedContacts.First();
                Assert.AreEqual(2, securedContacts.Count());

                IQueryable<DemoTask> securedTasks = dbContext.Tasks.Include(c => c.ContactTasks).ThenInclude(ct => ct.Contact);
                DemoTask securedTask = securedTasks.First();
                Assert.AreEqual(2, securedTasks.Count());
            }
        }
        
        // Write
        [Test]
        public void WriteObjectAllowPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Write, OperationState.Allow, criteria);

                obj1.Description = "Not good description";

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.Description = "Good description";
                dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void WriteObjectDenyPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Not good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Write, OperationState.Deny, criteria);

                obj1.Description = "Not good description";

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.Description = "Good description";
                dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void WriteObjectMultiplePermissions() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria = (db, obj) => obj.DecimalItem > 3;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Write, OperationState.Allow, goodCriteria);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria2 = (db, obj) => obj.DecimalItem < 9;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Write, OperationState.Allow, goodCriteria2);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.DecimalItem == 8;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Write, OperationState.Deny, badCriteria);

                obj1.DecimalItem = 8;
                try {
                    dbContextMultiClass.SaveChanges();
                    Assert.Fail("Fail");
                }
                catch(Exception e) {
                    // Assert.AreNotEqual("Fail", e.Message);
                    //bool isSecurityException = e.Message.StartsWith("Deny ");
                    Assert.AreNotEqual("Fail", e.Message);
                   // Assert.AreNotEqual("Fail", e.Message);
                }

                obj1.DecimalItem = 6;
                dbContextMultiClass.SaveChanges();
            }
        }     
        [Test]
        public void CreateObjectDenyPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                DbContextObject1 obj1 = new DbContextObject1();
                dbContextMultiClass.Add(obj1);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Not good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Create, OperationState.Deny, criteria);

                obj1.Description = "Not good description";

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.Description = "Good description";
                dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void CreateObjectMultiplePermissions() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                DbContextObject1 obj1 = new DbContextObject1();

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria = (db, obj) => obj.DecimalItem > 3;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Create, OperationState.Allow, goodCriteria);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria2 = (db, obj) => obj.DecimalItem < 9;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Create, OperationState.Allow, goodCriteria2);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.DecimalItem == 8;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Create, OperationState.Deny, badCriteria);       

                obj1.DecimalItem = 8;
                dbContextMultiClass.Add(obj1);

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.DecimalItem = 6;
                dbContextMultiClass.Add(obj1);
                dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void DeleteObjectCurrentValuesAllowPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Allow, criteria);

                obj1.Description = "Not good description";

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

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Allow, criteria);

                dbContextMultiClass.Remove(obj1);

                dbContextMultiClass.SaveChanges();
            }
        }        
        [Test]
        public void DeleteObjectDatabaseValuesAllowPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> deleteCriteria = (db, obj) => obj.Description == "Good description";
                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> writeCriteria = (db, obj) => true;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Allow, deleteCriteria);
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.ReadWrite, OperationState.Allow, writeCriteria);

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
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.ReadWrite, OperationState.Allow, writeCriteria);
                
                dbContextMultiClass.Remove(obj1);
                dbContextMultiClass.SaveChanges();
            }
            }
        [Test]
        public void DeleteObjectCurrentValuesDenyPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();     
                obj1.Description = "Not good description";
                dbContextMultiClass.SaveChanges(); //must save real value

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Not good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Deny, criteria);
                dbContextMultiClass.Remove(obj1);
                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.Description = "Good description";
                dbContextMultiClass.SaveChanges();

                dbContextMultiClass.Remove(obj1);
                dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void DeleteObjectDatabaseValuesDenyPermission() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Not good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Deny, criteria);

                obj1.Description = "Not good description";
                dbContextMultiClass.SaveChanges();

                dbContextMultiClass.Remove(obj1);

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.Description = "Good description";
                dbContextMultiClass.SaveChanges();
                dbContextMultiClass.Remove(obj1);
                dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void DeleteObjectMultiplePermissions() {
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();          
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj3 = new DbContextObject1();
                obj3.DecimalItem = 5;
                dbContextMultiClass.Add(obj3);
                dbContextMultiClass.SaveChanges();

                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();

                dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.AllowAllByDefault);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria = (db, obj) => obj.DecimalItem > 3;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Allow, goodCriteria);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria2 = (db, obj) => obj.DecimalItem < 9;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Allow, goodCriteria2);

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.DecimalItem == 8;
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Deny, badCriteria);
                
                obj1.DecimalItem = 8;
                dbContextMultiClass.SaveChanges();

                dbContextMultiClass.Remove(obj1);

                SecurityTestHelper.FailSaveChanges(dbContextMultiClass);

                obj1.DecimalItem = 5;
                dbContextMultiClass.SaveChanges();
                dbContextMultiClass.Remove(obj1);
                dbContextMultiClass.SaveChanges();
            }
        }
        // bad scenario
        [Test]
        public void DeleteObjectWithoutPermission() {
            int objectID = 0;
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                dbContextMultiClass.ResetDatabase();
                var obj = dbContextMultiClass.Add(new DbContextObject1());
                dbContextMultiClass.SaveChanges();
                DbContextObject1 obj1 = dbContextMultiClass.dbContextDbSet1.FirstOrDefault();
                objectID = obj1.ID;
                obj1.Description = "Not good description";
                dbContextMultiClass.SaveChanges();
            }
            using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {

                DbContextObject1 obj1 = new DbContextObject1( ) { ID = objectID };
                dbContextMultiClass.Entry(obj1).State = EntityState.Deleted;

                Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Not good description";
                dbContextMultiClass.PermissionsContainer.AddObjectPermission(SecurityOperation.Delete, OperationState.Deny, criteria);

                // obj1.Description = "Not good description";
                // dbContextMultiClass.SaveChanges();
    
                dbContextMultiClass.Remove(obj1);

                SecurityAccessException exception = SecurityTestHelper.FailSaveChanges(dbContextMultiClass) as SecurityAccessException;
                IList<BlockedObjectInfo> blockedInfoList = exception.GetBlockedInfo();
                Assert.AreEqual(1, blockedInfoList.Count);

                BlockedObjectInfo blockedInfo = blockedInfoList[0];
                Assert.AreEqual(null, blockedInfo.memberName);
                Assert.AreEqual(BlockedObjectInfo.OperationType.Delete, blockedInfo.operationType);
                Assert.AreEqual(typeof(DbContextObject1), blockedInfo.objectType);

                // obj1.Description = "Good description";
                // dbContextMultiClass.Remove(obj1);
                // dbContextMultiClass.SaveChanges();
            }
        }
        [Test]
        public void IsGrantedAllowPermission() {
            foreach(SecurityOperation securityOperation in new SecurityOperation[] {SecurityOperation.Read, SecurityOperation.Write, SecurityOperation.Delete, SecurityOperation.Create }) {
                using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                    dbContextMultiClass.ResetDatabase();
                    DbContextObject1 obj1 = new DbContextObject1();
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));

                    dbContextMultiClass.PermissionsContainer.SetPermissionPolicy(PermissionPolicy.DenyAllByDefault);

                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Good description";
                    dbContextMultiClass.PermissionsContainer.AddObjectPermission(securityOperation, OperationState.Allow, criteria);

                    obj1.Description = "Not good description";
                    Assert.IsFalse(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));

                    obj1.Description = "Good description";
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));
                }
            }
        }
        [Test]
        public void IsGrantedDenyPermission() {
            foreach(SecurityOperation securityOperation in new SecurityOperation[] { SecurityOperation.Read, SecurityOperation.Write, SecurityOperation.Delete, SecurityOperation.Create }) {
                using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                    dbContextMultiClass.ResetDatabase();
                    DbContextObject1 obj1 = new DbContextObject1();
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));

                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> criteria = (db, obj) => obj.Description == "Not good description";
                    dbContextMultiClass.PermissionsContainer.AddObjectPermission(securityOperation, OperationState.Deny, criteria);

                    obj1.Description = "Not good description";
                    Assert.IsFalse(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));

                    obj1.Description = "Good description";
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));
                }
            }
        }
        [Test]
        public void IsGrantedMultiplePermissions() {
            foreach(SecurityOperation securityOperation in new SecurityOperation[] { SecurityOperation.Read, SecurityOperation.Write, SecurityOperation.Delete, SecurityOperation.Create }) {
                using(DbContextMultiClass dbContextMultiClass = new DbContextMultiClass()) {
                    dbContextMultiClass.ResetDatabase();
                    DbContextObject1 obj1 = new DbContextObject1();
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), SecurityOperation.Write, obj1));

                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria = (db, obj) => obj.DecimalItem > 3;
                    dbContextMultiClass.PermissionsContainer.AddObjectPermission(securityOperation, OperationState.Allow, goodCriteria);

                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> goodCriteria2 = (db, obj) => obj.DecimalItem < 9;
                    dbContextMultiClass.PermissionsContainer.AddObjectPermission(securityOperation, OperationState.Allow, goodCriteria2);

                    Expression<Func<DbContextMultiClass, DbContextObject1, bool>> badCriteria = (db, obj) => obj.DecimalItem == 8;
                    dbContextMultiClass.PermissionsContainer.AddObjectPermission(securityOperation, OperationState.Deny, badCriteria);

                    obj1.DecimalItem = 8;
                    Assert.IsFalse(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));

                    obj1.DecimalItem = 5;
                    Assert.IsTrue(dbContextMultiClass.Security.IsGranted(typeof(DbContextObject1), securityOperation, obj1));
                }
            }
        }
    }

    [TestFixture]
    public class InMemoryObjectPermissionTests : ObjectPermissionTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.IN_MEMORY;
            base.ClearDatabase();
        }
    }

    [TestFixture]
    public class LocalDb2012ObjectPermissionTests : ObjectPermissionTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.LOCALDB_2012;
            base.ClearDatabase();
        }
    }
}
