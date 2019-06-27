namespace ServiceStack.Quartz.NetCore
{
    using System;
    using System.Collections.Specialized;
    using global::Funq;
    using global::Quartz;
    using ServiceStack;
    using ServiceStack.Quartz.NetCore.Quartz.Jobs;
    using ServiceStack.Quartz.ServiceInterface;
    using ServiceStack.Text;

    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost() : base("ServiceStackWithQuartz", typeof(CronDBService).Assembly)
        {
        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            SetConfig(new HostConfig {
                DebugMode = AppSettings.Get("DebugMode", Env.IsWindows),
            });

            // override config
            var quartzConfig = ConfigureQuartz();

            // register plugin, will scan assemblies by default for jobs
            var quartzFeature = new QuartzFeature();
            Plugins.Add(quartzFeature);
            Plugins.Add(new PostmanFeature());

            // or you can register the plugin with custom config source
            Plugins.AddIfNotExists(new QuartzFeature { Config = quartzConfig });

            // or you can register plugin with custom job assemblies
            Plugins.AddIfNotExists(new QuartzFeature
            {
                ScanAppHostAssemblies = false,
                JobAssemblies = new[] { typeof(HelloJob).Assembly, typeof(AppHost).Assembly }
            });

            // now you can setup a job to run with a trigger
            quartzFeature.RegisterJob<HelloJob>(
                trigger =>
                    trigger.WithSimpleSchedule(s =>
                            s.WithInterval(TimeSpan.FromMinutes(1))
                                .RepeatForever()
                        )
                        .Build()
            );

            // or setup a job to run with a trigger and some data
            quartzFeature.RegisterJob<HelloJob>(
                trigger =>
                    trigger.WithSimpleSchedule(s =>
                            s.WithIntervalInMinutes(1)
                                .WithRepeatCount(10)
                        )
                        .Build(),
                builder => builder.UsingJobData("Name", "Bob").Build()
            );
            
            // cron schedule trigger
            var cronTrigger = TriggerBuilder.Create()
                .WithCronSchedule("0/5 0 0 ? * * *")
                .Build();
            quartzFeature.RegisterJob<HelloJob>(cronTrigger);

            // you can setup jobs with data and triggers however you like
            // this lets create a trigger with our preferred identity
            var everyHourTrigger = TriggerBuilder.Create()
                .WithDescription("This is my trigger!")
                .WithIdentity("everyHour", "Continuous")
                .WithDailyTimeIntervalSchedule(x => x.OnMondayThroughFriday().WithIntervalInSeconds(5))
                .Build();

            var jobData = JobBuilder.Create<HelloJob>().UsingJobData("Name", "Sharon").Build();
            quartzFeature.RegisterJob<HelloJob>(everyHourTrigger);
            quartzFeature.RegisterJob<HelloJob>(everyHourTrigger, jobData);
        }

        /// <summary>
        /// Provides Quartz Configuration Settings
        /// </summary>
        /// <returns></returns>
        private NameValueCollection ConfigureQuartz()
        {
            /* set thread pool info */
            var properties =
                new NameValueCollection
                {
                    ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz",
                    ["quartz.threadPool.threadCount"] = "5",
                    ["quartz.threadPool.threadPriority"] = "Normal"
                };
            return properties;
        }
    }
}