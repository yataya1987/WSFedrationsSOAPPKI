﻿using NUnit.Framework;
using System;
using System.Linq;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using DevExpress.EntityFramework.SecurityDataStore.Tests.Helpers;
using DevExpress.EntityFramework.SecurityDataStore.Tests.DbContexts;

namespace DevExpress.EntityFramework.SecurityDataStore.Tests.Performance.Memory.Collections {
    [TestFixture]
    public abstract class CollectionsWriteTests {
        [Test]
        public void WriteObjectsWithoutPermissions() {
            WriteObjects(TestType.WithoutPermissions);
        }
        [Test]
        public void WriteObjectsWithOnePermission() {
            WriteObjects(TestType.WithOnePermission);
        }
        [Test]
        public void WriteObjectsWithMultiplePermissions() {
            WriteObjects(TestType.WithMultiplePermissions);
        }
        [Test]
        public void WriteObjects(TestType testType) {
            int count1 = 100;
            int count2 = 10;
            List<long> memoryUsages = new List<long>();
            List<Func<IDbContextConnectionClass>> contexts = PerformanceTestsHelper.GetMemoryTestsCollectionContextCreators();

            foreach(Func<IDbContextConnectionClass> createContext in contexts) {
                long initialUsedMemory = 0;
                long usedMemory = 0;

                initialUsedMemory = PerformanceTestsHelper.GetCurrentUsedMemory();

                using(IDisposable disposableContextInterface = (IDisposable)createContext()) {
                    IDbContextConnectionClass contextInterface = (IDbContextConnectionClass)disposableContextInterface;
                    DbContext context = (DbContext)contextInterface;
                    context.ResetDatabase();

                    for(int companyIndex = 0; companyIndex < count1; companyIndex++) {
                        string companySuffix = companyIndex.ToString();

                        Company company = new Company();
                        company.CompanyName = companySuffix;
                        company.Description = "Description" + companySuffix;

                        for(int officeIndex = 0; officeIndex < count2; officeIndex++) {
                            string officeSuffix = officeIndex.ToString();
                            Office office = new Office();
                            office.Name = officeSuffix;
                            office.Description = "Description" + companySuffix;

                            company.Offices.Add(office);
                        }

                        contextInterface.Company.Add(company);
                    }
                    context.SaveChanges();
                }

                using(IDisposable disposableContextInterface = (IDisposable)createContext()) {
                    IDbContextConnectionClass contextInterface = (IDbContextConnectionClass)disposableContextInterface;
                    DbContext context = (DbContext)contextInterface;

                    if(testType == TestType.WithOnePermission) {
                        SecurityDbContext securityDbContext = context as SecurityDbContext;
                        if(securityDbContext != null)
                            PerformanceTestsHelper.AddOneCollectionPermission(securityDbContext, SecurityOperation.Delete);
                    }

                    if(testType == TestType.WithMultiplePermissions) {
                        SecurityDbContext securityDbContext = context as SecurityDbContext;
                        if(securityDbContext != null)
                            PerformanceTestsHelper.AddMultipleCollectionPermissions(securityDbContext, SecurityOperation.Delete);
                    }                    

                    List<Company> objects = contextInterface.Company.Select(obj => obj).Include(obj => obj.Offices).ToList();
                    Assert.AreEqual(count1, objects.Count);
                    
                    for(int companyIndex = 0; companyIndex < count1; companyIndex++) {
                        Company company = objects[companyIndex];

                        for(int officeIndex = 0; officeIndex < count2 - 1; officeIndex += 2) {
                            Office curOffice = company.Offices[officeIndex];
                            Office nextOffice = company.Offices[officeIndex + 1];

                            company.Offices[officeIndex] = nextOffice;
                            company.Offices[officeIndex + 1] = curOffice;
                        }
                    }

                    context.SaveChanges();
                }

                long beforeCollect = GC.GetTotalMemory(true);
                usedMemory = PerformanceTestsHelper.GetCurrentUsedMemory();

                memoryUsages.Add(usedMemory - initialUsedMemory);
            }

            double securedContextBytesGrow = PerformanceTestsHelper.GetSecuredContextValue(memoryUsages);
            double nativeContextBytesGrow = PerformanceTestsHelper.GetNativeContextValue(memoryUsages);

            Assert.IsTrue(false, "our: " + securedContextBytesGrow.ToString() + " bytes, native: " + nativeContextBytesGrow.ToString() + " bytes");
        }
    }

    [TestFixture, Ignore("Memory tests are disabled")]
    public class InMemoryCollectionsWriteTests : CollectionsWriteTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.IN_MEMORY;
        }
    }

    [TestFixture, Ignore("Memory tests are disabled")]
    public class LocalDb2012CollectionsWriteTests : CollectionsWriteTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.LOCALDB_2012;
        }
    }

}
