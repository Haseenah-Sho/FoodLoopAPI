using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Features.Vendors.Commands
{
    public class ApproveRejectVendor
    {
        public record ApproveRejectVendorCommand(
            Guid VendorId,
            bool IsApproved) : IRequest<BaseResponse<Guid>>;

        public class ApproveRejectVendorValidator : AbstractValidator<ApproveRejectVendorCommand>
        {
            public ApproveRejectVendorValidator()
            {
                RuleFor(x => x.VendorId)
                    .NotEmpty().WithMessage("Vendor ID is required.");
            }
        }

        public class ApproveRejectVendorHandler(
            IVendorRepository vendorRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<ApproveRejectVendorCommand, BaseResponse<Guid>>
        {
           
                public async Task<BaseResponse<Guid>> Handle(
                    ApproveRejectVendorCommand request,
                    CancellationToken cancellationToken)
                {
                    try
                    {
                        var vendor = await vendorRepository.GetVendorAsync(request.VendorId)
                                  ?? await vendorRepository.GetVendorByUserIdAsync(request.VendorId);

                        if (vendor is null)
                            return BaseResponse<Guid>.Failure("Vendor not found.");

                        vendor.IsApproved = request.IsApproved;
                        vendor.DateModified = DateTime.UtcNow;

                        vendorRepository.Update(vendor);
                        await unitOfWork.SaveAsync();

                        var message = request.IsApproved
                            ? "Vendor has been approved successfully."
                            : "Vendor has been rejected successfully.";

                        return BaseResponse<Guid>.Success(message, vendor.Id);
                    }
                    catch (Exception ex)
                    {
                        return BaseResponse<Guid>.Failure(
                            $"An error occurred: {ex.Message}");
                    }
                }
            }
        }
    }
