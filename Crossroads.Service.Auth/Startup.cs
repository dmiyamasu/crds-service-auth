using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Crossroads.Service.Auth.Factories;
using Swashbuckle.AspNetCore.Swagger;
using Crossroads.Web.Common.Configuration;

namespace Crossroads.Service.Auth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                //TODO: Consider pulling version from project
                //TODO: Consider setting title to env variable for APP_NAME
                c.SwaggerDoc("v1", new Info { Title = "crds-service-auth", Version = "v1" });
            });

            // Register all the webcommon stuff
            CrossroadsWebCommonConfig.Register(services);

            //Add services
            OIDConfigurationFactory configurationFactory = new OIDConfigurationFactory();
            services.AddSingleton(configurationFactory);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "crds-service-auth");
                c.RoutePrefix = string.Empty;
            });

            // app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
