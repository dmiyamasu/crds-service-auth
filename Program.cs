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
using NLog.Targets;

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

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isDevelopment = environment == EnvironmentName.Development;

            var config = new LoggingConfiguration();

            //Set up to log to stdout
            if (isDevelopment)
            {
                var consoleTarget = new ColoredConsoleTarget("console")
                {
                    Layout = @"${date:format=HH\:mm\:ss} ${level} ${message} ${exception:format=ToString}"
                };
                config.AddTarget("console", consoleTarget);

                //Log everything in development
                config.AddRuleForAllLevels(consoleTarget, "*");
            }
            else // Log to Logzio
            {
                var logzioTarget = new LogzioTarget
                {
                    Token = Environment.GetEnvironmentVariable("LOGZ_IO_KEY"),
                };
                logzioTarget.ContextProperties.Add(new TargetPropertyWithContext("host", "${machinename}"));
                logzioTarget.ContextProperties.Add(new TargetPropertyWithContext("application", "auth"));
                logzioTarget.ContextProperties.Add(new TargetPropertyWithContext("environment", Environment.GetEnvironmentVariable("CRDS_ENV")));
                config.AddTarget("logzio", logzioTarget);

                //Log only warn and above for all built in logs
                config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, logzioTarget, "*");

                //Log everything debug and above for custom logs
                config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logzioTarget, "Crossroads.*");
            }

            LogManager.Configuration = config;

            var logger = NLogBuilder.ConfigureNLog(config).GetCurrentClassLogger();

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
