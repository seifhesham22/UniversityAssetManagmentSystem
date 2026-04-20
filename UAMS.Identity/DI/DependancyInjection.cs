using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Identity.IdentityModels;
using UAMS.Identity.Presistence;

namespace UAMS.Identity.DI
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddIdentityModule(
            this IServiceCollection services,
            IConfiguration configs)
        {
            services.AddDbContext<AuthDbContext>(x =>
            {
                x.UseNpgsql(configs.GetConnectionString("default"));
            });

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}