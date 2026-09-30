using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetVendorPickupPoints
    {
        public record GetVendorPickupPointsQuery(Guid VendorUserId) : IRequest<BaseResponse<List<PickupPointDto>>>;

        public record PickupPointDto(Guid PointId, string PointName, string Address);

        public class GetVendorPickupPointsHandler(IVendorRepository vendorRepository)
            : IRequestHandler<GetVendorPickupPointsQuery, BaseResponse<List<PickupPointDto>>>
        {
            public async Task<BaseResponse<List<PickupPointDto>>> Handle(
                GetVendorPickupPointsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorWithPickupPointsAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<List<PickupPointDto>>.Failure("Food Provider's profile not found.");

                    var points = vendor.PickupPoints
                        .Select(p => new PickupPointDto(p.Id, p.PointName, p.Address))
                        .ToList();

                    return BaseResponse<List<PickupPointDto>>.Success("Retrieved successfully.", points);
                }
                catch (Exception ex)
                {
                    return BaseResponse<List<PickupPointDto>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}