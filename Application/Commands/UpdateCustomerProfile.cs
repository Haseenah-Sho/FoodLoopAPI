using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class UpdateCustomerProfile
    {
        public record UpdateCustomerProfileCommand(
            Guid UserId,
            string PhoneNumber,
            string Address) : IRequest<BaseResponse<UpdateCustomerProfileResponse>>;

        public class UpdateCustomerProfileValidator : AbstractValidator<UpdateCustomerProfileCommand>
        {
            public UpdateCustomerProfileValidator()
            {
                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required.")
                    .Matches(@"^(\+?\d{1,3}[-\s]?)?0?\d{10}$")
                    .WithMessage("Enter a valid phone number.");

                RuleFor(x => x.Address)
                    .NotEmpty().WithMessage("Address is required.")
                    .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");
            }
        }

        public class UpdateCustomerProfileHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<UpdateCustomerProfileCommand, BaseResponse<UpdateCustomerProfileResponse>>
        {
            public async Task<BaseResponse<UpdateCustomerProfileResponse>> Handle(
                UpdateCustomerProfileCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.UserId);
                    if (customer is null)
                        return BaseResponse<UpdateCustomerProfileResponse>.Failure("Customer profile not found.");

                    customer.PhoneNumber = request.PhoneNumber;
                    customer.Address = request.Address;
                    customer.DateModified = DateTime.UtcNow;

                    customerRepository.Update(customer);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<UpdateCustomerProfileResponse>.Success(
                        "Profile updated successfully.",
                        new UpdateCustomerProfileResponse(
                            customer.Id,
                            customer.PhoneNumber,
                            customer.Address));
                }
                catch (Exception ex)
                {
                    return BaseResponse<UpdateCustomerProfileResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }

        public record UpdateCustomerProfileResponse(
            Guid CustomerId,
            string PhoneNumber,
            string Address);
    }
}