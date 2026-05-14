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
using UAMS.Campus.FacadeImplementations;
using UAMS.Campus.Features;
using UAMS.Campus.Presistence;
using UAMS.Identity.Facades;
using UAMS.Room.Facades;
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

            services.AddScoped<IFacultyFacade, FacultyFacade>();
            services.AddScoped<IGetCampusUser, GetCampusUser>();

            return services;
        }
    }
}