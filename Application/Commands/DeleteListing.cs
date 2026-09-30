using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Commands
{
    public class DeleteListing
    {
        public record DeleteListingCommand(Guid VendorUserId, Guid ListingId)
            : IRequest<BaseResponse<string>>;

        public class DeleteListingHandler(
            IVendorRepository vendorRepository,
            IListingRepository listingRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<DeleteListingCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(
                DeleteListingCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<string>.Failure("Vendor profile not found.");

                    var listing = await listingRepository.GetListingAsync(request.ListingId);
                    if (listing is null)
                        return BaseResponse<string>.Failure("Food item not found.");

                    if (listing.VendorId != vendor.Id)
                        return BaseResponse<string>.Failure("You are not authorized to delete this food item.");

                    listing.IsDeleted = true;
                    listing.DateModified = DateTime.UtcNow;
                    listingRepository.Update(listing);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<string>.Success("Food item deleted successfully.", listing.Id.ToString());
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}