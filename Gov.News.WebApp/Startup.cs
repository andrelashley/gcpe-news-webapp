﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Gov.News.WebApp;
using Gov.News.Website.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Gov.News.Website
{
    public class Startup
    {
        // test date for granville age
        public static DateTime granvilleTestDate;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);
            //    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            if (env.IsDevelopment())
                builder.AddUserSecrets<Startup>();
            if (!System.Diagnostics.Debugger.IsAttached)
                builder.AddEnvironmentVariables(); // for openshift

            Configuration = builder.Build();

            Configuration.Bind(Properties.Settings.Default);

            if (Configuration["GranvilleTestDate"] != null)
            {
                try
                {
                    granvilleTestDate = DateTime.Parse(Configuration["GranvilleTestDate"]);
                }
                catch (SystemException)
                {
                }
            }

            //Data.Repository.RepositoryException += (ex) => Program.ReportException(null, ex);
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.

            services.AddMemoryCache();

            services.AddMvc(opt =>
                {
                    opt.EnableEndpointRouting = false;
                })
                .AddMvcOptions(options =>
            {
#if DEBUG
                var cacheProfile = new CacheProfile { Location = ResponseCacheLocation.None, NoStore = true };
#else
                var cacheProfile = new CacheProfile { Duration = 60 };
#endif

                options.CacheProfiles.Add("Default", cacheProfile);
                options.CacheProfiles.Add("Feed", cacheProfile);
                options.CacheProfiles.Add("Embed", cacheProfile);
                options.CacheProfiles.Add("Page", cacheProfile);
                options.CacheProfiles.Add("Archive", cacheProfile);

                options.Filters.Add(new TypeFilterAttribute(typeof(XFrameOptionsAttribute)));

                options.Filters.Add(new TypeFilterAttribute(typeof(RequirePermanentHttpsAttribute)));

                options.Filters.Add(new ExceptionReportingFilter());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //TODO: Change to ServiceLifetime.Scoped once repository is no longer using static methods
            services.AddSingleton(new Func<IServiceProvider, Gov.News.Api.IClient>((serviceProvider) =>
            {
                var client = new Gov.News.Api.Client();
                client.BaseUri = new Uri(Configuration["NewsApi"]);
                return client;
            }));



            /*
            services.AddSingleton(new Func<IServiceProvider, Gcpe.Hub.Services.Legacy.INewslettersClient>((serviceProvider) =>
            {
                var client = new Gcpe.Hub.Services.Legacy.NewslettersClient();
                client.BaseUri = new Uri(Configuration.GetConnectionString("HubNewslettersClient"));
                return client;
            }));


            services.Configure<Data.RepositoryOptions>(Configuration.GetSection("Options:Gov.News.Data:Repository"));
                */


            services.AddSingleton<Repository, Repository>();
            services.AddSingleton<IHostedService, Hubs.LiveHub>();

            // Add the Configuration object so that controllers may use it through dependency injection
            services.AddSingleton<IConfiguration>(Configuration);

            services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimestampLogger<>)));

            services
                .AddHealthChecks()
                .AddAsyncCheck("/hc", async () =>
                {
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            try
                            {
                                var response = await client.GetAsync(new Uri(Configuration["NewsApi"] + "hc"));
                                if (!response.IsSuccessStatusCode)
                                {
                                    throw new Exception("Url not responding with 200 OK");
                                }
                            }
                            catch (Exception)
                            {
                                return await Task.FromResult(HealthCheckResult.Unhealthy());
                            }
                        }
                        return await Task.FromResult(HealthCheckResult.Healthy());
                    }

                }, new string[] { "health-check" });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseHealthChecks("/hc", new HealthCheckOptions { AllowCachingResponses = false });

            app.UseRedirect();

            // set headers for static files
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=600";
                    ctx.Context.Response.Headers[HeaderNames.Pragma] = "no-cache";
                    ctx.Context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                    ctx.Context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                    ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                }
            });

#if USE_JAVASCRIPT_SIGNALR
            if (Properties.Settings.Default.SignalREnabled != null && Properties.Settings.Default.SignalREnabled=="true")
            {
                app.UseSignalR();
            }
#endif
            app.UseMvc(routes =>
            {
                routes.RegisterRoutes();
            });
        }
    }
}