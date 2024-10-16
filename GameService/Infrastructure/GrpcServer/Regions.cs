using Game.Contracts;
using Grpc.Core;
using GrpcConfiguration.Region;
namespace Game.Infrastructure.GrpcServer
{
    public class Regions : GrpcRegions.GrpcRegionsBase
    {
        private readonly IRepositoryManager _repositoryManager;

        public Regions(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public override async Task<RegionsResponse> GetAllRegions(GetAllRegionslRequest request, ServerCallContext context)
        {
            var response = new RegionsResponse();
            var regions = await _repositoryManager.RegionRepository.GetAllAsync();

            foreach (var region in regions)
            {
                response.Regions.Add(new GrpcRegionModel { Name = region.Name });
            }


            return response;
        }

    }
}




