using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Crossroads.Service.Auth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ReadEnvironmentVariables();
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

        static void ReadEnvironmentVariables()
        {
            //TODO: Autogenerate a file, whether its .env or .json is TBD

            try
            {
                DotNetEnv.Env.Load("../.env");
            }
            catch (Exception)
            {
                // no .env file present but since not required, just write
                Console.WriteLine("no .env file found, reading environment variables from machine");
            }
        }
    }
}
