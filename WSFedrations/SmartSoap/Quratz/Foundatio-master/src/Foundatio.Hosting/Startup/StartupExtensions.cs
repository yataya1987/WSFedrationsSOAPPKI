﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundatio.Hosting.Startup {
    public static partial class StartupExtensions {
        public static async Task<bool> RunStartupActionsAsync(this IServiceProvider serviceProvider, CancellationToken shutdownToken = default) {
            var sw = Stopwatch.StartNew();
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("StartupActions") ?? NullLogger.Instance;
            var startupActions = serviceProvider.GetServices<StartupActionRegistration>().ToArray();
            logger.LogInformation("Found {StartupActions} registered startup action(s).", startupActions.Length);
            
            var startupActionPriorityGroups = startupActions.GroupBy(s => s.Priority).OrderBy(s => s.Key).ToArray();
            foreach (var startupActionGroup in startupActionPriorityGroups) {
                int startupActionsCount = startupActionGroup.Count();
                string[] startupActionsNames = startupActionGroup.Select(a => a.Name).ToArray();
                var swGroup = Stopwatch.StartNew();
                try {
                    if (startupActionsCount == 1)
                        logger.LogInformation("Running {StartupActions} (priority {Priority}) startup action...", startupActionsNames, startupActionGroup.Key);
                    else
                        logger.LogInformation("Running {StartupActions} (priority {Priority}) startup actions in parallel...", startupActionsNames, startupActionGroup.Key);

                    await Task.WhenAll(startupActionGroup.Select(async a => {
                        try {
                            await a.RunAsync(serviceProvider, shutdownToken).AnyContext();
                        } catch (Exception ex) {
                            logger.LogError(ex, "Error running {StartupAction} startup action: {Message}", a.Name, ex.Message);
                            throw;
                        }
                    })).AnyContext();
                    swGroup.Stop();

                    if (startupActionsCount == 1)
                        logger.LogInformation("Completed {StartupActions} startup action in {Duration:mm\\:ss}.", startupActionsNames, swGroup.Elapsed);
                    else
                        logger.LogInformation("Completed {StartupActions} startup actions in {Duration:mm\\:ss}.", startupActionsNames, swGroup.Elapsed);
                } catch {
                    return false;
                }
            }
            
            sw.Stop();
            logger.LogInformation("Completed all {StartupActions} startup action(s) in {Duration:mm\\:ss}.", startupActions.Length, sw.Elapsed);
            return true;
        }

        public static void AddStartupAction<T>(this IServiceCollection services, int? priority = null) where T : IStartupAction {
            services.TryAddSingleton<StartupContext>();
            if (!services.Any(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(RunStartupActionsService)))
                services.AddSingleton<IHostedService, RunStartupActionsService>();
            services.TryAddTransient(typeof(T));
            services.AddTransient(s => new StartupActionRegistration(typeof(T).Name, typeof(T), priority));
        }

        public static void AddStartupAction<T>(this IServiceCollection services, string name, int? priority = null) where T : IStartupAction {
            services.TryAddSingleton<StartupContext>();
            if (!services.Any(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(RunStartupActionsService)))
                services.AddSingleton<IHostedService, RunStartupActionsService>();
            services.TryAddTransient(typeof(T));
            services.AddTransient(s => new StartupActionRegistration(name, typeof(T), priority));
        }

        public static void AddStartupAction(this IServiceCollection services, string name, Action action, int? priority = null) {
            services.AddStartupAction(name, ct => action(), priority);
        }

        public static void AddStartupAction(this IServiceCollection services, string name, Action<IServiceProvider> action, int? priority = null) {
            services.AddStartupAction(name, (sp, ct) => action(sp), priority);
        }

        public static void AddStartupAction(this IServiceCollection services, string name, Action<IServiceProvider, CancellationToken> action, int? priority = null) {
            services.AddTransient(s => new StartupActionRegistration(name, (sp, ct) => {
                action(sp, ct);
                return Task.CompletedTask;
            }, priority));
        }

        public static void AddStartupAction(this IServiceCollection services, string name, Func<Task> action, int? priority = null) {
            services.AddStartupAction(name, (sp, ct) => action(), priority);
        }

        public static void AddStartupAction(this IServiceCollection services, string name, Func<IServiceProvider, Task> action, int? priority = null) {
            services.AddStartupAction(name, (sp, ct) => action(sp), priority);
        }

        public static void AddStartupAction(this IServiceCollection services, string name, Func<IServiceProvider, CancellationToken, Task> action, int? priority = null) {
            services.TryAddSingleton<StartupContext>();
            if (!services.Any(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(RunStartupActionsService)))
                services.AddSingleton<IHostedService, RunStartupActionsService>();
            services.AddTransient(s => new StartupActionRegistration(name, action, priority));
        }

        public static IHealthChecksBuilder AddCheckForStartupActionsComplete(this IHealthChecksBuilder builder) {
            return builder.AddCheck<StartupHealthCheck>("Startup");
        }

        public static IApplicationBuilder UseWaitForStartupActionsBeforeServingRequests(this IApplicationBuilder builder) {
            return builder.UseMiddleware<WaitForStartupActionsBeforeServingRequestsMiddleware>();
        }

        public static void AddStartupActionToWaitForHealthChecks(this IServiceCollection services, Func<HealthCheckRegistration, bool> shouldWaitForHealthCheck = null) {
            if (shouldWaitForHealthCheck == null)
                shouldWaitForHealthCheck = c => c.Tags.Contains("Critical", StringComparer.OrdinalIgnoreCase);

            services.AddStartupAction("WaitForHealthChecks", async (sp, t) => {
                var healthCheckService = sp.GetService<HealthCheckService>();
                var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("StartupActions") ?? NullLogger.Instance;
                var result = await healthCheckService.CheckHealthAsync(c => c.GetType() != typeof(StartupHealthCheck) && shouldWaitForHealthCheck(c), t).AnyContext();
                while (result.Status == HealthStatus.Unhealthy) {
                    logger.LogDebug("Last health check was unhealthy. Waiting 1s until next health check.");
                    await Task.Delay(1000, t).AnyContext();
                    result = await healthCheckService.CheckHealthAsync(c => c.GetType() != typeof(StartupHealthCheck) && shouldWaitForHealthCheck(c), t).AnyContext();
                }
            }, -100);
        }
    }
}
