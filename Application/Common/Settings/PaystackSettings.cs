namespace Application.Common.Models
{
    public class PaystackSettings
    {
        public string SecretKey { get; set; } = default!;
        public string BaseUrl { get; set; } = default!;
        public decimal PlatformCommissionPercentage { get; set; } = 5;
        public string CallbackUrl { get; set; } = default!;
    }
}