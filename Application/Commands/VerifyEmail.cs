using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;

namespace Application.Features.Customers.Commands
{
    public class VerifyEmail
    {
        public record VerifyEmailCommand(
            string Email,
            string Token) : IRequest<BaseResponse<string>>;

        public class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
        {
            public VerifyEmailValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("A valid email address is required.");

                RuleFor(x => x.Token)
                    .NotEmpty().WithMessage("Verification token is required.");
            }
        }

        public class VerifyEmailHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<VerifyEmailCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(
                VerifyEmailCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var user = await userRepository.GetAsync(request.Email);
                    if (user is null)
                        return BaseResponse<string>.Failure("User not found.");

                    if (user.IsEmailVerified)
                        return BaseResponse<string>.Failure("Email is already verified.");

                    if (user.VerificationToken != request.Token)
                        return BaseResponse<string>.Failure("Invalid verification token.");

                    if (user.VerificationTokenExpiryTime < DateTime.UtcNow)
                        return BaseResponse<string>.Failure(
                            "Verification token has expired. Please request a new one.");

                    user.IsEmailVerified = true;
                    user.VerificationToken = null;
                    user.VerificationTokenExpiryTime = null;
                    user.DateModified = DateTime.UtcNow;

                    userRepository.Update(user);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<string>.Success(
                        "Email verified successfully. You can now log in.",
                        user.Id.ToString());
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure(
                        $"An error occurred during email verification: {ex.Message}");
                }
            }
        }
    }
}