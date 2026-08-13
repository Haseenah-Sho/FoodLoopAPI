using Domain.Enums;

namespace Domain.Entities
{
    public class Listing : BaseEntity
    {
        public Guid VendorId { get; set; }
        public Vendor Vendor { get; set; } = default!;
        public string FoodName { get; set; } = default!;
        public string FoodDescription { get; set; } = default!;
        public int Quantity { get; set; }
        public int QuantityPerUnit { get; set; }
        public int RemainingPortion { get; set; }
        public bool IsFree { get; set; }
        public decimal? Price { get; set; }
        public bool PickUpAvailable { get; set; }
        public bool DeliveryAvailable { get; set; }
        public DateTime PickUpStart { get; set; }
        public DateTime PickUpEnd { get; set; }
        public ListingStatus Status { get; set; }
        public ICollection<Rating> Ratings { get; set; } = new HashSet<Rating>();
        public ICollection<ListingImage> ListingImages { get; set; } = new HashSet<ListingImage>();
        public string Address { get; set; } = default!;
        public FoodType FoodType { get; set; }
        public StorageInstruction StorageInstruction { get; set; }
        public string? Allergens { get; set; }
        public DateTime BestBeforeDate { get; set; }
    }
}