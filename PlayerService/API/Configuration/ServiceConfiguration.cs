using Application.Mapping;
using Application.Services;
using Contracts;
using Domain.Entities;
using Domain.Models;
using Infrastructure;
using Infrastructure.Context;
using Infrastructure.Services;
using Match.Infrastructure.GrpcClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

namespace API.Extensions
{
    public static class ServiceConfiguration
    {
        public static void ConfigureSqlServer(this IServiceCollection services, IConfiguration configuration,string key) => 
            services.AddDbContext<ApplicationDbContext>(options =>
                                            options.UseSqlServer(configuration.GetConnectionString(key), options =>
                                            {
                                                options.MigrationsAssembly("Player.API");
                                            }));

        public static void ConfigureIdentityService(this IServiceCollection services, IConfiguration config)
        {

            string Issuer = config.GetValue<string>("ApiSettings:JwtOptions:Issuer");
            string TokenKey = config.GetValue<string>("ApiSettings:JwtOptions:Secret");

            services.AddIdentity<User, Role>(option =>
            {
                option.Password.RequireNonAlphanumeric = false;
                option.User.RequireUniqueEmail = true;


            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();




            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKey)),
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidIssuer = Issuer,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                   
                    options.Events = new JwtBearerEvents
                    {
                        
                        OnAuthenticationFailed = context =>
                        {
                            Log.Error("Authentication failed: {0}", context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Log.Information("Token validated for: {0}", context.Principal.Identity.Name);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Log.Warning("OnChallenge error: {0}", context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                });
        }
        
        public static void GeneralConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services.AddTransient<ITokenGenerator, TokenGenerator>();
            services.AddSingleton<IGamemodeService, GamemodeService>();
            services.AddSingleton<IEventPublisher, EventPublisher>();
            services.AddScoped<IAuthService, AuthorizationService>();
            services.Configure<JwtOptions>(config.GetSection("ApiSettings:JwtOptions"));
        }

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void ConfigureAutomapper(this IServiceCollection services) => 
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
        
        public static void ConfigureSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(options =>
            {

                options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter the Bearer Authorization string example: `Bearer Generated-JWT-Token`",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });

                options.AddSecurityRequirement(
                new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        new string[] {}
                    }
                });

            });
        }

        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.AllowAnyOrigin()
                                                    .AllowAnyMethod()
                                                    .AllowAnyHeader());
            });
    }
}
