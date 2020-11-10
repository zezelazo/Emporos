using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Emporos.Core.Data;

namespace Emporos.Api
{
    /// <summary>
    /// Program class entry point for the app
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">command line params</param>
        public static void Main(string[] args)
        {
            var logger = NLogBuilder
                .ConfigureNLog("nlog.config")
                .GetCurrentClassLogger();
            try
            {
                var host = CreateHostBuilder(args).Build();

                try
                {
                    SeedDb(host);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "An error occurred while seeding the database.");
                }

                host.Run();

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application stopped because of exception.");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        private static void SeedDb(IHost host)
        {
            var scopeFactory = host.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var seeder = scope.ServiceProvider.GetService<IDataSeeder>();
            seeder.Seed();
        }
        /// <summary>
        /// Build web api host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    //logging.AddAzureWebAppDiagnostics();
                });
    }
}
