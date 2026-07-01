using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllCustomers
    {
        public record GetAllCustomersQuery() : IRequest<BaseResponse<List<GetAllCustomersResponse>>>;

        public class GetAllCustomersHandler(
            ICustomerRepository customerRepository)
            : IRequestHandler<GetAllCustomersQuery, BaseResponse<List<GetAllCustomersResponse>>>
        {
            public async Task<BaseResponse<List<GetAllCustomersResponse>>> Handle(
                GetAllCustomersQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customers = await customerRepository.GetAllCustomersAsync();

                    var response = customers
                        .Where(c => !c.IsDeleted)
                        .Select(c => new GetAllCustomersResponse(
                            c.Id,
                            c.User.FullName ?? c.User.UserName,
                            c.User.Email,
                            c.PhoneNumber,
                            c.Address,
                            c.Orders.Count,
                            c.DateCreated))
                        .ToList();

                    return BaseResponse<List<GetAllCustomersResponse>>.Success(
                        "Customers retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<List<GetAllCustomersResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetAllCustomersResponse(
            Guid CustomerId,
            string Name,
            string Email,
            string PhoneNumber,
            string Address,
            int TotalOrders,
            DateTime DateJoined);
    }
}