using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class ManagePickupPoints
    {
        public record AddPickupPointCommand(Guid VendorUserId, string PointName, string Address)
            : IRequest<BaseResponse<Guid>>;

        public record RemovePickupPointCommand(Guid VendorUserId, Guid PointId)
            : IRequest<BaseResponse<string>>;

        public class AddPickupPointValidator : AbstractValidator<AddPickupPointCommand>
        {
            public AddPickupPointValidator()
            {
                RuleFor(x => x.PointName).NotEmpty().MaximumLength(100)
                    .WithMessage("Location name is required and cannot exceed 100 characters.");
                RuleFor(x => x.Address).NotEmpty().MaximumLength(300)
                    .WithMessage("Address is required and cannot exceed 300 characters.");
            }
        }

        public class AddPickupPointHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
            : IRequestHandler<AddPickupPointCommand, BaseResponse<Guid>>
        {
            public async Task<BaseResponse<Guid>> Handle(AddPickupPointCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorWithPickupPointsAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<Guid>.Failure("Vendor profile not found.");

                    if (vendor.PickupPoints.Any(p => p.PointName.Equals(request.PointName, StringComparison.OrdinalIgnoreCase)))
                        return BaseResponse<Guid>.Failure("You already have a location with this name.");

                    var point = new VendorPickupPoint
                    {
                        VendorId = vendor.Id,
                        PointName = request.PointName,
                        Address = request.Address,
                        CreatedBy = vendor.User.Email
                    };

                    await vendorRepository.AddPickupPointAsync(point);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<Guid>.Success("Pickup location added.", point.Id);
                }
                catch (Exception ex)
                {
                    return BaseResponse<Guid>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public class RemovePickupPointHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
            : IRequestHandler<RemovePickupPointCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(RemovePickupPointCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorWithPickupPointsAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<string>.Failure("Food Provider's profile not found.");

                    var point = vendor.PickupPoints.FirstOrDefault(p => p.Id == request.PointId);
                    if (point is null)
                        return BaseResponse<string>.Failure("Location not found.");

                    point.IsDeleted = true;
                    point.DateModified = DateTime.UtcNow;
                    await unitOfWork.SaveAsync();

                    return BaseResponse<string>.Success("Pickup location removed.", "Removed");
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}