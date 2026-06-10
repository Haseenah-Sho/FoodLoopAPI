using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RatingRepository(AppDbContext context) : IRatingRepository
    {
        public async Task AddAsync(Rating rating)
        {
            await context.Set<Rating>().AddAsync(rating);
        }

        public async Task<ICollection<Rating>> GetByListingAsync(Guid listingId)
        {
            return await context.Set<Rating>()
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .Where(r => r.ListingId == listingId && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<Rating>> GetByVendorAsync(Guid vendorId)
        {
            return await context.Set<Rating>()
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .Where(r => r.VendorId == vendorId && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> HasCustomerRatedListingAsync(Guid customerId, Guid listingId)
        {
            return await context.Set<Rating>()
                .AnyAsync(r => r.CustomerId == customerId
                            && r.ListingId == listingId
                            && !r.IsDeleted);
        }

        public async Task<bool> HasCustomerRatedVendorAsync(Guid customerId, Guid vendorId)
        {
            return await context.Set<Rating>()
                .AnyAsync(r => r.CustomerId == customerId
                            && r.VendorId == vendorId
                            && !r.IsDeleted);
        }
    }
}