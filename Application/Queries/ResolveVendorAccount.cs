using Application.Common.Dtos;
using Application.Services;
using MediatR;

namespace Application.Queries
{
    public class ResolveVendorAccount
    {
        public record ResolveVendorAccountQuery(string AccountNumber, string BankCode)
            : IRequest<BaseResponse<string>>;

        public class ResolveVendorAccountHandler(IPaystackService paystackService)
            : IRequestHandler<ResolveVendorAccountQuery, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(
                ResolveVendorAccountQuery request, CancellationToken cancellationToken)
            {
                var result = await paystackService.ResolveAccountNumberAsync(request.AccountNumber, request.BankCode);

                if (!result.Success || string.IsNullOrWhiteSpace(result.AccountName))
                    return BaseResponse<string>.Failure(result.ErrorMessage ?? "Could not verify this account number.");

                return BaseResponse<string>.Success("Account verified.", result.AccountName);
            }
        }
    }
}