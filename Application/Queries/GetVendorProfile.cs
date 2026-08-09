using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetVendorProfile
    {
        public record GetVendorProfileQuery(Guid UserId) : IRequest<BaseResponse<VendorProfileResponse>>;

        public record VendorProfileResponse(
            Guid VendorId,
            string OrganizationName,
            string PhoneNumber,
            string Email,
            bool IsApproved);

        public class GetVendorProfileHandler(
            IVendorRepository vendorRepository)
            : IRequestHandler<GetVendorProfileQuery, BaseResponse<VendorProfileResponse>>
        {
            public async Task<BaseResponse<VendorProfileResponse>> Handle(
                GetVendorProfileQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.UserId);
                    if (vendor is null)
                        return BaseResponse<VendorProfileResponse>.Failure("Vendor profile not found.");

                    return BaseResponse<VendorProfileResponse>.Success(
                        "Profile retrieved successfully.",
                        new VendorProfileResponse(
                            vendor.Id,
                            vendor.OrganizationName,
                            vendor.PhoneNumber,
                            vendor.User.Email,
                            vendor.IsApproved));
                }
                catch (Exception ex)
                {
                    return BaseResponse<VendorProfileResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}