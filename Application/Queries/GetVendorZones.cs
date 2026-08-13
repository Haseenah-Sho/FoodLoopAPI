using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetVendorZones
    {
        public record GetVendorZonesQuery(Guid VendorUserId) : IRequest<BaseResponse<List<ZoneDto>>>;

        public record ZoneDto(Guid ZoneId, string ZoneName, decimal Fee);

        public class GetVendorZonesHandler(IVendorRepository vendorRepository)
            : IRequestHandler<GetVendorZonesQuery, BaseResponse<List<ZoneDto>>>
        {
            public async Task<BaseResponse<List<ZoneDto>>> Handle(
                GetVendorZonesQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorWithZonesAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<List<ZoneDto>>.Failure("Food Provider profile not found.");

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