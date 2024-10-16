using Grpc.Net.Client;
using GrpcConfiguration.Region;
using Match.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Match.Infrastructure.GrpcClient;

public class RegionService : IRegionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegionService> _logger;

    public RegionService(IConfiguration configuration,ILogger<RegionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    public IEnumerable<string> GetRegions()
    {
        _logger.LogInformation($"Calling GRPC Service {_configuration["GrpcPlatform"]}");


        var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
        var client = new GrpcRegions.GrpcRegionsClient(channel);
        var request = new GetAllRegionslRequest();

        try
        {
            var reply = client.GetAllRegions(request);
            return reply.Regions.Select(r=> r.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Couldnot call GRPC Server {ex.Message}");
            return null;
        }
    }
}
