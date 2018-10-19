using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Logzio.DotNet.NLog;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using NLog.Config;

namespace Crossroads.Service.Auth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                DotNetEnv.Env.Load(".env");
            }
            catch (Exception)
            {
                // no .env file present but since not required, just write
                Console.WriteLine("no .env file found, reading environment variables from machine");
            }

            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            var config = new LoggingConfiguration();
            var logzioTarget = new LogzioTarget
            {
                Token = Environment.GetEnvironmentVariable("LOGZ_IO_KEY"),
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                   .UseNLog();  // NLog: setup NLog for Dependency injection
    }
}
