using Application.Common.Dtos;
using Application.Common.Helpers;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class CreateListing
    {
        public class CreateListingCommand : IRequest<BaseResponse<CreateListingResponse>>
        {
            public Guid VendorUserId { get; set; }
            public string FoodName { get; set; } = default!;
            public string FoodDescription { get; set; } = default!;
            public int Quantity { get; set; }
            public int QuantityPerUnit { get; set; }
            public bool IsFree { get; set; }
            public decimal? Price { get; set; }
            public bool PickUpAvailable { get; set; }
            public bool DeliveryAvailable { get; set; }
            public DateTime PickUpStart { get; set; }
            public DateTime PickUpEnd { get; set; }
            public decimal DeliveryFee { get; set; }
            public List<IFormFile> Images { get; set; } = new();
        }

        public class CreateListingValidator : AbstractValidator<CreateListingCommand>
        {
            public CreateListingValidator()
            {
                RuleFor(x => x.FoodName)
                    .NotEmpty().WithMessage("Food name is required.")
                    .MaximumLength(150).WithMessage("Food name cannot exceed 150 characters.");

                RuleFor(x => x.FoodDescription)
                    .NotEmpty().WithMessage("Food description is required.")
                    .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

                RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

                RuleFor(x => x.QuantityPerUnit)
                    .GreaterThan(0).WithMessage("Quantity per unit must be greater than zero.");

                RuleFor(x => x.Price)
                    .GreaterThan(0).WithMessage("Price must be greater than zero.")
                    .When(x => !x.IsFree);

                RuleFor(x => x.PickUpStart)
                    .LessThan(x => x.PickUpEnd).WithMessage("Pickup start time must be before pickup end time.");

                RuleFor(x => x)
                    .Must(x => x.PickUpAvailable || x.DeliveryAvailable)
                    .WithMessage("At least one fulfilment option (pickup or delivery) must be available.");

                RuleFor(x => x.DeliveryFee)
                    .GreaterThanOrEqualTo(0).WithMessage("Delivery fee cannot be negative.")
                    .When(x => x.DeliveryAvailable);

                RuleFor(x => x.Images)
                    .NotEmpty().WithMessage("At least one image is required.")
                    .Must(images => images.Count <= 5).WithMessage("You can upload a maximum of 5 images.");
            }
        }

        public class CreateListingHandler(
            IVendorRepository vendorRepository,
            IListingRepository listingRepository,
            IUnitOfWork unitOfWork,
            IFileUploadService fileUploadService,
            INotificationService notificationService)
            : IRequestHandler<CreateListingCommand, BaseResponse<CreateListingResponse>>
        {
            public async Task<BaseResponse<CreateListingResponse>> Handle(
                CreateListingCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<CreateListingResponse>.Failure("Vendor profile not found.");

                    if (!vendor.IsApproved)
                        return BaseResponse<CreateListingResponse>.Failure(
                            "Your vendor account is not yet approved. You cannot create listings until approved by admin.");

                    foreach (var image in request.Images)
                    {
                        var (isValid, errorMessage) = FileValidationHelper.Validate(image.FileName, image.Length);
                        if (!isValid)
                            return BaseResponse<CreateListingResponse>.Failure(errorMessage!);
                    }

                    var listing = new Listing
                    {
                        VendorId = vendor.Id,
                        FoodName = request.FoodName,
                        FoodDescription = request.FoodDescription,
                        Quantity = request.Quantity,
                        QuantityPerUnit = request.QuantityPerUnit,
                        RemainingPortion = request.Quantity,
                        IsFree = request.IsFree,
                        Price = request.IsFree ? null : request.Price,
                        PickUpAvailable = request.PickUpAvailable,
                        DeliveryAvailable = request.DeliveryAvailable,
                        PickUpStart = request.PickUpStart,
                        PickUpEnd = request.PickUpEnd,
                        DeliveryFee = request.DeliveryAvailable ? request.DeliveryFee : 0,
                        Status = ListingStatus.Active,
                        CreatedBy = vendor.User.Email,
                    };

                    for (int i = 0; i < request.Images.Count; i++)
                    {
                        var image = request.Images[i];
                        using var stream = image.OpenReadStream();
                        var imageUrl = await fileUploadService.UploadAsync(stream, image.FileName, "listings");

                        listing.ListingImages.Add(new ListingImage
                        {
                            ImageUrl = imageUrl,
                            IsPrimary = i == 0,
                            CreatedBy = vendor.User.Email,
                        });
                    }

                    await listingRepository.AddAsync(listing);
                    await unitOfWork.SaveAsync();

                    // Broadcast to anyone browsing listings right now
                    await notificationService.NotifyNewListing(
                        listing.Id,
                        listing.FoodName,
                        vendor.OrganizationName,
                        listing.ListingImages.FirstOrDefault(li => li.IsPrimary)?.ImageUrl);


                    return BaseResponse<CreateListingResponse>.Success(
                        "Listing created successfully.",
                        new CreateListingResponse(
                            listing.Id,
                            listing.FoodName,
                            listing.Status.ToString(),
                            listing.ListingImages.Select(li => li.ImageUrl).ToList()));
                }
                catch (Exception ex)
                {
                    return BaseResponse<CreateListingResponse>.Failure(
                        $"An error occurred while creating the listing: {ex.Message}");
                }
            }
        }

        public record CreateListingResponse(
            Guid ListingId,
            string FoodName,
            string Status,
            List<string> ImageUrls);
    }
}