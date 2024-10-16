using Match.API.RabbitMQ;
using Match.Application.Services;
using Match.Contracts;
using Match.Infrastructure.GrpcClient;
using Match.Infrastructure.Services;

namespace API.Extensions
{
    public static class ServiceConfiguration
    {

        public static void GeneralConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IRegionService, RegionService>();
            services.AddSingleton<IGamemodeService, GamemodeService>();
            services.AddSingleton<IEventProcessor, EventProcessor>();
            services.AddSingleton<IEventPublisher, EventPublisher>();


        }

        public static void ConfigureMatchMakingBackgroundService(this IServiceCollection services) 
            => services.AddHostedService<MatchMakingEventHandler>();

    }
}
