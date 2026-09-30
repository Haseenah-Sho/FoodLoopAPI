using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetCustomerProfile
    {
        public record GetCustomerProfileQuery(Guid UserId) : IRequest<BaseResponse<CustomerProfileResponse>>;

        public record CustomerProfileResponse(
            Guid CustomerId,
            string Name,
            string Email,
            string PhoneNumber,
            string Address);

        public class GetCustomerProfileHandler(
            ICustomerRepository customerRepository)
            : IRequestHandler<GetCustomerProfileQuery, BaseResponse<CustomerProfileResponse>>
        {
            public async Task<BaseResponse<CustomerProfileResponse>> Handle(
                GetCustomerProfileQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.UserId);
                    if (customer is null)
                        return BaseResponse<CustomerProfileResponse>.Failure("Customer profile not found.");

                    return BaseResponse<CustomerProfileResponse>.Success(
                        "Profile retrieved successfully.",
                        new CustomerProfileResponse(
                            customer.Id,
                            customer.User.FullName ?? "",
                            customer.User.Email,
                            customer.PhoneNumber,
                            customer.Address));
                }
                catch (Exception ex)
                {
                    return BaseResponse<CustomerProfileResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}