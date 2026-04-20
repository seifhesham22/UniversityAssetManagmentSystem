using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UAMS.Campus.Behaviours;
using UAMS.Campus.Features;
using UAMS.Campus.Presistence;
namespace UAMS.Campus.DI
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddCampusModule(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CampusDbContext>(x =>
            {
                x.UseNpgsql(configuration.GetConnectionString("default"));
            });

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            });

            services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }
    }
}