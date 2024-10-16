using GameSession.API.EventHandling;
using GameSession.API.RabbitMQ;
using GameSession.Application.MediatR;
using GameSession.Application.Services;
using GameSession.Contracts;
using GameSession.Contracts.Service;
using GameSession.Infrastructure;
using GameSession.Infrastructure.Context;
using GameSession.Infrastructure.GrpcClient;
using GameSession.Infrastructure.Mapping;
using GameSession.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using System.Text;

namespace GameSession.API.Extensions
{
    public static class ServiceConfiguration
    {

        public static void ConfigureMongoDb(this IServiceCollection services, IConfiguration configuration) =>
            services.AddSingleton<MongoDbContext>(sp =>
            {
                var client = new MongoClient(configuration.GetConnectionString("MongoDbConnection"));
                return new MongoDbContext(client, "GameSessionDb"); ;
            });
        public static void GeneralConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IEventPublisher, EventPublisher>();
            services.AddSingleton<IGamesessionService, GamesessionService>();
            services.AddSingleton<IPlayerService,PlayerService>();
        }

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration config)
        {
            string issuer = config.GetValue<string>("ApiSettings:JwtOptions:Issuer");
            string secret = config.GetValue<string>("ApiSettings:JwtOptions:Secret");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.UseSecurityTokenValidators = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidIssuer = issuer,
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
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our SignalR hubs
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/GameSessionHub"))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }

                };
            });


        }
        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();


        public static void ConfigureAutomapper(this IServiceCollection services) => 
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

        public static void ConfigureGameSessionBackgroundService(this IServiceCollection services) => 
            services.AddHostedService<GameSessionEventHandler>();
       
        public static void ConfigureGameSessionmMonitoringBackgroundService(this IServiceCollection services) => 
            services.AddHostedService<GameSessionMonitoring>();
       
        public static void ConfigureMediatR(this IServiceCollection services) =>
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(MediatRConfiguration).Assembly));

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
