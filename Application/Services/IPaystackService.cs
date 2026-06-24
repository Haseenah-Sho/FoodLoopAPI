namespace Application.Services
{
    public record InitializePaymentResult(bool Success, string? AuthorizationUrl, string? Reference, string? ErrorMessage);
    public record VerifyPaymentResult(bool Success, bool IsPaid, decimal AmountPaid, string? ErrorMessage);

    public interface IPaystackService
    {
        Task<InitializePaymentResult> InitializeTransactionAsync(string email, decimal amount, string reference);
        Task<VerifyPaymentResult> VerifyTransactionAsync(string reference);
    }
}