namespace ServiceStack.Quartz.Tests
{
    using System;
    using FluentAssertions;
    using ServiceStack;
    using ServiceStack.Quartz.ServiceInterface;
    using ServiceStack.Quartz.ServiceModel;
    using ServiceStack.Testing;
    using Xunit;

    public class UnitTestAccessDB : IDisposable
    {
        private readonly ServiceStackHost appHost;

        public UnitTestAccessDB()
        {
            appHost = new BasicAppHost(typeof(CronDBService).Assembly)
            {
                ConfigureContainer = container =>
                {
                    //Add your IoC dependencies here
                }
            }
            .Init();
        }

        [Fact]
        public void TestMethodDB()
        {
            var service = appHost.Container.Resolve<CronDBService>();

            var response = (HelloResponse)service.Any(new Hello { Name = "World" });

            response.Result.Should().Be("Hello, World!");
        }

        public void Dispose()
        {
            appHost?.Dispose();
        }
    }
}
