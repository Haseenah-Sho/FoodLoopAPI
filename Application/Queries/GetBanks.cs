using Application.Common.Dtos;
using Application.Services;
using MediatR;

namespace Application.Queries
{
    public class GetBanks
    {
        public record GetBanksQuery() : IRequest<BaseResponse<List<BankInfo>>>;

        public class GetBanksHandler(IPaystackService paystackService)
            : IRequestHandler<GetBanksQuery, BaseResponse<List<BankInfo>>>
        {
            public async Task<BaseResponse<List<BankInfo>>> Handle(
                GetBanksQuery request, CancellationToken cancellationToken)
            {
                var result = await paystackService.ListBanksAsync();

                if (!result.Success)
                    return BaseResponse<List<BankInfo>>.Failure(result.ErrorMessage ?? "Could not load banks.");

                return BaseResponse<List<BankInfo>>.Success("Banks retrieved successfully.", result.Banks);
            }
        }
    }
}