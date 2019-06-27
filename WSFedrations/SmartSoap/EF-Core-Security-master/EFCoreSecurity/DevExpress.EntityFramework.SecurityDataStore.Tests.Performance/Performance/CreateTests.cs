﻿using NUnit.Framework;
using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using DevExpress.EntityFramework.SecurityDataStore.Tests.Helpers;
using DevExpress.EntityFramework.SecurityDataStore.Tests.DbContexts;

namespace DevExpress.EntityFramework.SecurityDataStore.Tests.Performance {
    [TestFixture]
    public abstract class CreateTests : BasePerformanceTestClass {
        [Test]
        public void CreateObjectsWithoutPermissions() {
            CreateObjects(TestType.WithoutPermissions);
        }
        [Test]
        public void CreateObjectsWithOnePermission() {
            CreateObjects(TestType.WithOnePermission);
        }
        [Test]
        public void CreateObjectsWithMultiplePermissions() {
            CreateObjects(TestType.WithMultiplePermissions);
        }

        public void CreateObjects(TestType testType) {
            int count = 1000;
            List<long> times = new List<long>();
            List<Func<IDbContextMultiClass>> contexts = PerformanceTestsHelper.GetContextCreators();

            foreach(Func<IDbContextMultiClass> createContext in contexts) {
                IDbContextMultiClass contextInterface = createContext();
                DbContext context = (DbContext)contextInterface;
                context.ResetDatabase();

                if(testType == TestType.WithOnePermission) {
                    SecurityDbContext securityDbContext = context as SecurityDbContext;
                    if(securityDbContext != null)
                        PerformanceTestsHelper.AddOnePermission(securityDbContext, SecurityOperation.Create);
                }

                if(testType == TestType.WithMultiplePermissions) {
                    SecurityDbContext securityDbContext = context as SecurityDbContext;
                    if(securityDbContext != null)
                        PerformanceTestsHelper.AddMultiplePermissions(securityDbContext, SecurityOperation.Create);
                }

                Stopwatch watch = new Stopwatch();
                watch.Start();

                for(int i = 0; i < count; i++) {
                    DbContextObject1 obj = new DbContextObject1();
                    obj.Description = "Description " + i.ToString();
                    context.Add(obj);
                }
                context.SaveChanges();
                watch.Stop();
                times.Add(watch.ElapsedMilliseconds);
            }

            double securedContextTime = PerformanceTestsHelper.GetSecuredContextValue(times);
            double nativeContextTime = PerformanceTestsHelper.GetNativeContextValue(times);

            double nominalTimeDifference = GetTimeDifference(testType);
            double timeDifference = securedContextTime - nativeContextTime;
            Assert.IsTrue(timeDifference <= nominalTimeDifference, GetTimeDifferenceErrorString(timeDifference, nominalTimeDifference));
            Debug.WriteLine(GetDebugTimeString(securedContextTime, nativeContextTime));
        }
    }

    [TestFixture]
    public class InMemoryCreateTests : CreateTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.IN_MEMORY;

            SetTimeDifference(TestType.WithoutPermissions, 180);
            SetTimeDifference(TestType.WithOnePermission, 180);
            SetTimeDifference(TestType.WithMultiplePermissions, 180);
        }
    }

    [TestFixture]
    public class LocalDb2012CreateTests : CreateTests {
        [SetUp]
        public void Setup() {
            SecurityTestHelper.CurrentDatabaseProviderType = SecurityTestHelper.DatabaseProviderType.LOCALDB_2012;

            SetTimeDifference(TestType.WithoutPermissions, 200);
            SetTimeDifference(TestType.WithOnePermission, 250);
            SetTimeDifference(TestType.WithMultiplePermissions, 300);
        }
    }
}
