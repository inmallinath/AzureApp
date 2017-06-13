using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AzureApp.Data;
using AzureApp.Models;
using AzureApp.Services;

namespace AzureApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _env = env;
        }

        public IConfigurationRoot Configuration { get; }
        private IHostingEnvironment _env { get; }
        private string _connectionString = String.Empty;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
           
            if (_env.IsDevelopment())
            {
                _connectionString = Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connectionString));
            }
            else if (_env.IsStaging())
            {
                _connectionString = Environment.GetEnvironmentVariable("PSQL_CONNECTION_STRING");
                //_connectionString = Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_connectionString));
            }
            else if(_env.IsProduction())
            {
                _connectionString = Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING");
                services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_connectionString));
            }
            else
            {
                 _connectionString = Configuration.GetConnectionString("DefaultConnection");
            }
            

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
