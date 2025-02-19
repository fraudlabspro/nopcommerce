﻿using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.FraudLabsPro.Factories;
using Nop.Plugin.Misc.FraudLabsPro.Services;

namespace Nop.Plugin.Misc.FraudLabsPro.Infrastructure
{
    /// <summary>
    /// Represents a FraudLabs Pro dependency registrar
    /// </summary>
    public class DependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        /*public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //register custom services
            builder.RegisterType<FraudLabsProManager>().AsSelf().InstancePerLifetimeScope();

            //register custom factories
            builder.RegisterType<FraudLabsProOrderModelFactory>().AsSelf().InstancePerLifetimeScope();
        }*/
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //register custom services
            services.AddScoped<FraudLabsProManager>();

            //register custom factories
            services.AddScoped<FraudLabsProOrderModelFactory>(); 
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 2;
    }
}
