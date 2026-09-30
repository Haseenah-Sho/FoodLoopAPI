using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetVendor
    {
        public record GetVendorQuery(Guid UserId) : IRequest<BaseResponse<GetVendorResponse>>;

        public record GetVendorResponse(
            Guid VendorId,
            string OrganizationName,
            string PhoneNumber,
            string Email,
            bool IsApproved,
            DateTime RegisteredOn);

        public class GetVendorHandler(
            IVendorRepository vendorRepository)
            : IRequestHandler<GetVendorQuery, BaseResponse<GetVendorResponse>>
        {
            public async Task<BaseResponse<GetVendorResponse>> Handle(
                GetVendorQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.UserId);
                    if (vendor is null)
                        return BaseResponse<GetVendorResponse>.Failure("Vendor not found.");

                    var response = new GetVendorResponse(
                        vendor.Id,
                        vendor.OrganizationName,
                        vendor.PhoneNumber,
                        vendor.User.Email,
                        vendor.IsApproved,
                        vendor.DateCreated);

                    return BaseResponse<GetVendorResponse>.Success(
                        "Vendor retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<GetVendorResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}