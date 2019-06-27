namespace ServiceStack.Quartz.ServiceInterface
{
    using ServiceStack;
    using ServiceStack.Quartz.ServiceModel;

    public class CronDBService : Service
    {
        public HelloResponse Any(Hello request)
        {
            return new HelloResponse { Result = "Hello, {0}!".Fmt(request.Name) };
        }
    }
}