using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class UpdateVendorProfile
    {
        public record UpdateVendorProfileCommand(
            Guid UserId,
            string OrganizationName,
            string PhoneNumber,
            string Address) : IRequest<BaseResponse<UpdateVendorProfileResponse>>;

        public class UpdateVendorProfileValidator : AbstractValidator<UpdateVendorProfileCommand>
        {
            public UpdateVendorProfileValidator()
            {
                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required.")
                    .Matches(@"^(\+?\d{1,3}[-\s]?)?0?\d{10}$")
                    .WithMessage("Enter a valid phone number.");

                RuleFor(x => x.Address)
                    .NotEmpty().WithMessage("Address is required.")
                    .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");
            }
        }

        public class UpdateVendorProfileHandler(
            IVendorRepository vendorRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<UpdateVendorProfileCommand, BaseResponse<UpdateVendorProfileResponse>>
        {
            public async Task<BaseResponse<UpdateVendorProfileResponse>> Handle(
                UpdateVendorProfileCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.UserId);
                    if (vendor is null)
                        return BaseResponse<UpdateVendorProfileResponse>.Failure("Vendor profile not found.");

                    bool organizationNameChanged = vendor.OrganizationName != request.OrganizationName;

                    vendor.OrganizationName = request.OrganizationName;
                    vendor.PhoneNumber = request.PhoneNumber;
                    vendor.Address = request.Address;
                    vendor.DateModified = DateTime.UtcNow;

                    string message = "Profile updated successfully.";

                    if (organizationNameChanged)
                    {
                        vendor.IsApproved = false;
                        message = "Profile updated. Since your organization name changed, your account is pending admin re-approval.";
                    }

                    vendorRepository.Update(vendor);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<UpdateVendorProfileResponse>.Success(
                        message,
                        new UpdateVendorProfileResponse(
                            vendor.Id, vendor.OrganizationName, vendor.PhoneNumber, vendor.Address, vendor.IsApproved));
                }
                catch (Exception ex)
                {
                    return BaseResponse<UpdateVendorProfileResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }

        public record UpdateVendorProfileResponse(
            Guid Id, string OrganizationName, string PhoneNumber, string Address, bool IsApproved);
    }
}