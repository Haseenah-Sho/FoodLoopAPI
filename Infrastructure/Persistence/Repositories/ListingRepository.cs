using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ListingRepository(AppDbContext context) : IListingRepository
    {
        public async Task AddAsync(Listing listing)
        {
            await context.Set<Listing>().AddAsync(listing);
        }

        public void Update(Listing listing)
        {
            context.Set<Listing>().Update(listing);
        }

        public async Task<Listing?> GetListingAsync(Guid id)
        {
            return await context.Set<Listing>()
                .Include(l => l.Vendor)
                    .ThenInclude(v => v.User)
                .Include(l => l.Ratings)
                .Include(l => l.ListingImages)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        }

        public async Task<ICollection<Listing>> GetAllActiveListingsAsync()
        {
            return await context.Set<Listing>()
                .Include(l => l.Vendor)
                    .ThenInclude(v => v.User)
                .Include(l => l.ListingImages)
                .Where(l => l.Status == ListingStatus.Active && !l.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<Listing>> GetListingsByVendorAsync(Guid vendorId)
        {
            return await context.Set<Listing>()
                .Include(l => l.ListingImages)
                .Where(l => l.VendorId == vendorId && !l.IsDeleted)
                .OrderByDescending(l => l.DateCreated)
                .ToListAsync();
        }

        public async Task<bool> TryDecrementStockAsync(Guid listingId, int quantity)
        {
            var rowsAffected = await context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Listings 
            SET RemainingPortion = RemainingPortion - {quantity},
            Status = CASE WHEN (RemainingPortion - {quantity}) <= 0 THEN 'Completed' ELSE Status END
            WHERE Id = {listingId} AND RemainingPortion >= {quantity} AND IsDeleted != 1");

            return rowsAffected > 0;
        }

        public async Task RestoreStockAsync(Guid listingId, int quantity)
        {
            await context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Listings 
            SET RemainingPortion = RemainingPortion + {quantity},
            Status = CASE WHEN Status = 'Completed' THEN 'Active' ELSE Status END
            WHERE Id = {listingId}");
        }

        public async Task<ICollection<Listing>> GetAllListingsAsync()
        {
            return await context.Set<Listing>()
                .Include(l => l.Vendor)
                .ToListAsync();
        }
    }
}