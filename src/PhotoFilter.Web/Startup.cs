using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using PhotoFilter.Web.Infrastructure;
using StackExchange.Redis;

namespace PhotoFilter.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var storageAccount = CloudStorageAccount.Parse(Configuration.GetSection("Storage")["ConnectionString"]);
            services.AddSingleton( _ =>
            {
                return storageAccount.CreateCloudBlobClient();
            });

            services.AddSingleton(_ =>
           {
               return storageAccount.CreateCloudQueueClient();
           });

            services.AddSingleton(_ =>
            {
                return ConnectionMultiplexer.Connect("highscore.redis.cache.windows.net,abortConnect=false,ssl=true,password=bm74xhq66IbzLtCnkWj+34thhsV0pEIYsKDZStJje1E=");
            });

            services.AddScoped<ImageStore>();
            services.AddScoped<ImageLease>();
            services.AddScoped<ImageCount>();

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
