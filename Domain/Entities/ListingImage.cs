namespace Domain.Entities
{
    public class ListingImage : BaseEntity
    {
        public Guid ListingId { get; set; }
        public Listing Listing { get; set; } = default!;
        public string ImageUrl { get; set; } = default!;
        public bool IsPrimary { get; set; } = false;
    }
}