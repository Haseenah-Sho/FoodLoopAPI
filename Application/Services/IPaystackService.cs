namespace Application.Services
{
    public record InitializePaymentResult(bool Success, string? AuthorizationUrl, string? Reference, string? ErrorMessage);
    public record VerifyPaymentResult(bool Success, bool IsPaid, decimal AmountPaid, string? ErrorMessage);
    public record ResolveAccountResult(bool Success, string? AccountName, string? ErrorMessage);
    public record CreateSubaccountResult(bool Success, string? SubaccountCode, string? ErrorMessage);
    public record BankInfo(string Name, string Code);
    public record ListBanksResult(bool Success, List<BankInfo> Banks, string? ErrorMessage);

    public interface IPaystackService
    {
        Task<InitializePaymentResult> InitializeTransactionAsync(string email, decimal amount, string reference, string? subaccountCode = null);
        Task<VerifyPaymentResult> VerifyTransactionAsync(string reference);
        Task<ResolveAccountResult> ResolveAccountNumberAsync(string accountNumber, string bankCode);
        Task<CreateSubaccountResult> CreateSubaccountAsync(string businessName, string bankCode, string accountNumber, decimal percentageCharge);
        Task<ListBanksResult> ListBanksAsync();
    }
}