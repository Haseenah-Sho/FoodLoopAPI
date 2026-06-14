using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Features.Vendors.Queries
{
    public class GetPendingVendors
    {
        public record GetPendingVendorsQuery() : IRequest<BaseResponse<ICollection<PendingVendorResponse>>>;

        public record PendingVendorResponse(
            Guid VendorId,
            string OrganizationName,
            string PhoneNumber,
            string Email,
            DateTime RegisteredOn);

        public class GetPendingVendorsHandler(
            IVendorRepository vendorRepository)
            : IRequestHandler<GetPendingVendorsQuery, BaseResponse<ICollection<PendingVendorResponse>>>
        {
            public async Task<BaseResponse<ICollection<PendingVendorResponse>>> Handle(
                GetPendingVendorsQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendors = await vendorRepository.GetPendingVendorsAsync();

                    var response = vendors.Select(v => new PendingVendorResponse(
                        v.Id,
                        v.OrganizationName,
                        v.PhoneNumber,
                        v.User.Email,
                        v.DateCreated)).ToList();

                    return BaseResponse<ICollection<PendingVendorResponse>>.Success(
                        "Pending vendors retrieved successfully.",
                        response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<ICollection<PendingVendorResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}