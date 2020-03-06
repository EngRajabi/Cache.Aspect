using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using Cache.Aspect.Service;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cache.Aspect
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; private set; }

        public ILifetimeScope AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);

            services.AddDistributedMemoryCache();

            // Add services to the collection
            services.AddOptions();
            
            

            // Create a container-builder and register dependencies
            var builder = new ContainerBuilder();

            // Populate the service-descriptors added to `IServiceCollection`
            // BEFORE you add things to Autofac so that the Autofac
            // registrations can override stuff in the `IServiceCollection`
            // as needed
            builder.Populate(services);

            builder.RegisterType<TestService>()
                .As<ITestService>()
                .InstancePerLifetimeScope()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(CacheInterceptor)); ;

            // Typed registration
            //builder.Register(c => new CacheInterceptor());
            builder.RegisterType<CacheInterceptor>();

            // Register your own things directly with AutofacD:\Projects\Cache.Aspect\Cache.Aspect\Startup.cs
            //builder.RegisterModule(new MyApplicationModule());

            AutofacContainer = builder.Build();

            // this will be used as the service-provider for the application!
            return new AutofacServiceProvider(AutofacContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseMvcWithDefaultRoute();
        }
    }
}