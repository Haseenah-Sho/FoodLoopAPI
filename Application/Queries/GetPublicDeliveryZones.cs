using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetPublicDeliveryZones
    {
        public record GetPublicDeliveryZonesQuery(Guid VendorId) : IRequest<BaseResponse<List<ZoneDto>>>;

        public record ZoneDto(Guid ZoneId, string ZoneName, decimal Fee);

        public class GetPublicDeliveryZonesHandler(IVendorRepository vendorRepository)
            : IRequestHandler<GetPublicDeliveryZonesQuery, BaseResponse<List<ZoneDto>>>
        {
            public async Task<BaseResponse<List<ZoneDto>>> Handle(
                GetPublicDeliveryZonesQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorWithZonesByVendorIdAsync(request.VendorId);
                    if (vendor is null)
                        return BaseResponse<List<ZoneDto>>.Failure("Food Provider not found.");

                    var zones = vendor.DeliveryZones
                        .Select(z => new ZoneDto(z.Id, z.ZoneName, z.Fee))
                        .ToList();

                    return BaseResponse<List<ZoneDto>>.Success("Retrieved successfully.", zones);
                }
                catch (Exception ex)
                {
                    return BaseResponse<List<ZoneDto>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}