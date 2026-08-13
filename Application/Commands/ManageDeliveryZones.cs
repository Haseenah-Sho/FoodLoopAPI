using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class ManageDeliveryZones
    {
        public record AddDeliveryZoneCommand(Guid VendorUserId, string ZoneName, decimal Fee)
            : IRequest<BaseResponse<Guid>>;

        public record RemoveDeliveryZoneCommand(Guid VendorUserId, Guid ZoneId)
            : IRequest<BaseResponse<string>>;

        public class AddDeliveryZoneValidator : AbstractValidator<AddDeliveryZoneCommand>
        {
            public AddDeliveryZoneValidator()
            {
                RuleFor(x => x.ZoneName).NotEmpty().MaximumLength(100)
                    .WithMessage("Zone name is required and cannot exceed 100 characters.");
                RuleFor(x => x.Fee).GreaterThanOrEqualTo(0)
                    .WithMessage("Fee cannot be negative.");
            }
        }

        public class AddDeliveryZoneHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
            : IRequestHandler<AddDeliveryZoneCommand, BaseResponse<Guid>>
        {
            public async Task<BaseResponse<Guid>> Handle(AddDeliveryZoneCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorWithZonesAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<Guid>.Failure("Food Provider Profile not found.");

                    if (vendor.DeliveryZones.Any(z => z.ZoneName.Equals(request.ZoneName, StringComparison.OrdinalIgnoreCase)))
                        return BaseResponse<Guid>.Failure("You already have a zone with this name.");

                    var zone = new VendorDeliveryZone
                    {
                        VendorId = vendor.Id,
                        ZoneName = request.ZoneName,
                        Fee = request.Fee,
                        CreatedBy = vendor.User.Email
                    };

                    await vendorRepository.AddDeliveryZoneAsync(zone);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<Guid>.Success("Delivery zone added.", zone.Id);
                }
                catch (Exception ex)
                {
                    return BaseResponse<Guid>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public class RemoveDeliveryZoneHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
            : IRequestHandler<RemoveDeliveryZoneCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(RemoveDeliveryZoneCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorWithZonesAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<string>.Failure("Food Provider profile not found.");

                    var zone = vendor.DeliveryZones.FirstOrDefault(z => z.Id == request.ZoneId);
                    if (zone is null)
                        return BaseResponse<string>.Failure("Zone not found.");

                    zone.IsDeleted = true;
                    zone.DateModified = DateTime.UtcNow;
                    await unitOfWork.SaveAsync();

                    return BaseResponse<string>.Success("Delivery zone removed.", "Removed");
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}