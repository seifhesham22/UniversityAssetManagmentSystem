using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Identity.IdentityModels;
using UAMS.Identity.Presistence;
using UAMS.Identity.Services.AuthService;
using UAMS.Identity.Services.TokenService;

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

            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 10;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.SuperAdminOnly, p =>
                    p.RequireClaim(AppClaims.Role, Role.SuperAdmin.ToString()));

                options.AddPolicy(Policies.AssetManagerOnly, p =>
                    p.RequireClaim(AppClaims.Role, Role.AssetManager.ToString()));

                options.AddPolicy(Policies.DepartmentManagerOnly, p =>
                    p.RequireClaim(AppClaims.Role, Role.DepartmentManager.ToString()));

                options.AddPolicy(Policies.MaintainerOnly, p =>
                    p.RequireClaim(AppClaims.Role, Role.Maintainer.ToString()));

                options.AddPolicy(Policies.TeacherOnly, p =>
                    p.RequireClaim(AppClaims.Role, Role.Teacher.ToString()));

                options.AddPolicy(Policies.StudentOnly, p =>
                    p.RequireClaim(AppClaims.Role, Role.Student.ToString()));

                options.AddPolicy(Policies.CanReportIssue, p =>
                    p.RequireClaim(AppClaims.Role,
                        Role.Student.ToString(),
                        Role.Teacher.ToString()));

                options.AddPolicy(Policies.CanConfirmFix, p =>
                    p.RequireClaim(AppClaims.Role,
                        Role.Teacher.ToString(),
                        Role.AssetManager.ToString()));

                options.AddPolicy(Policies.CanManageTicket, p =>
                    p.RequireClaim(AppClaims.Role,
                        Role.AssetManager.ToString()));

                options.AddPolicy(Policies.CanSubmitInspection, p =>
                    p.RequireClaim(AppClaims.Role,
                        Role.Maintainer.ToString()));

                options.AddPolicy(Policies.CanDesignRoom, p =>
                    p.RequireClaim(AppClaims.Role,
                        Role.AssetManager.ToString(),
                        Role.Teacher.ToString()));

                options.AddPolicy(Policies.CanViewRoomDesign, p =>
                    p.RequireClaim(AppClaims.Role,
                        Role.Student.ToString(),
                        Role.Teacher.ToString(),
                        Role.AssetManager.ToString()));

                options.AddPolicy(Policies.CanCloseRoom, p =>
                    p.RequireClaim(AppClaims.Role,
                        Role.DepartmentManager.ToString()));

                options.AddPolicy(Policies.CanManageAssets, p =>
                    p.RequireClaim(AppClaims.Role,
                        Role.AssetManager.ToString(),
                        Role.SuperAdmin.ToString()));

                options.AddPolicy(Policies.CanCheckChecklist, p =>
                    p.RequireClaim(AppClaims.Role,
                        Role.AssetManager.ToString(),
                        Role.Maintainer.ToString()));

                options.AddPolicy(Policies.CanViewFaculties, p =>
                    p.RequireClaim(AppClaims.Role,
                    Role.Student.ToString(),
                    Role.SuperAdmin.ToString(),
                    Role.AssetManager.ToString(),
                    Role.Teacher.ToString()));
            });

            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<ITokenService, TokenHandler>();

            return services;
        }
    }
}