using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class VendorRepository(AppDbContext context) : IVendorRepository
    {
        public async Task AddAsync(Vendor vendor)
        {
            await context.Set<Vendor>().AddAsync(vendor);
        }

        public void Update(Vendor vendor)
        {
            context.Set<Vendor>().Update(vendor);
        }

        public async Task<Vendor?> GetVendorAsync(Guid id)
        {
            return await context.Set<Vendor>()
                .Include(v => v.User)
                .Include(v => v.Listings)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<Vendor?> GetVendorByUserIdAsync(Guid userId)
        {
            return await context.Set<Vendor>()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.UserId == userId && !v.IsDeleted);
        }

        public async Task<ICollection<Vendor>> GetAllVendorsAsync()
        {
            return await context.Set<Vendor>()
                .Include(v => v.User)
                .Where(v => !v.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<Vendor>> GetPendingVendorsAsync()
        {
            return await context.Set<Vendor>()
                .Include(v => v.User)
                .Where(v => !v.IsApproved && !v.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsExistAsync(Guid userId)
        {
            return await context.Set<Vendor>()
                .AnyAsync(v => v.UserId == userId && !v.IsDeleted);
        }

        public async Task<Vendor?> GetVendorWithZonesAsync(Guid userId)
        {
            return await context.Set<Vendor>()
                .Include(v => v.User)
                .Include(v => v.DeliveryZones.Where(z => !z.IsDeleted))
                .FirstOrDefaultAsync(v => v.UserId == userId && !v.IsDeleted);
        }

        public async Task AddDeliveryZoneAsync(VendorDeliveryZone zone)
        {
            await context.Set<VendorDeliveryZone>().AddAsync(zone);
        }

        public async Task<Vendor?> GetVendorWithPickupPointsAsync(Guid userId)
        {
            return await context.Set<Vendor>()
                .Include(v => v.User)
                .Include(v => v.PickupPoints.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(v => v.UserId == userId && !v.IsDeleted);
        }

        public async Task AddPickupPointAsync(VendorPickupPoint point)
        {
            await context.Set<VendorPickupPoint>().AddAsync(point);
        }

        public async Task<Vendor?> GetVendorWithZonesByVendorIdAsync(Guid vendorId)
        {
            return await context.Set<Vendor>()
                .Include(v => v.DeliveryZones.Where(z => !z.IsDeleted))
                .FirstOrDefaultAsync(v => v.Id == vendorId && !v.IsDeleted);
        }
    }
}