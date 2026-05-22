using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;
using UAMS.Room.VkBot;

namespace UAMS.Room.DI
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddRoomDesignModule(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RoomDesignDbContext>(x =>
            {
                x.UseNpgsql(configuration.GetConnectionString("default"));
                x.LogTo(Console.WriteLine);
            });

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            });

            services.AddMemoryCache();
            services.AddHttpClient("vk");
            services.AddScoped<IVkBotService, VkBotService>();

            return services;
        }
    }
}