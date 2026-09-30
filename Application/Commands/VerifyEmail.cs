using Application.Common.Dtos;
using Application.Repositories;
using FluentValidation;
using MediatR;
using static Application.Commands.VerifyEmail.VerifyEmailHandler;

namespace Application.Commands
{
    public class VerifyEmail
    {
        public record VerifyEmailCommand(
            string Email,
            string Token) : IRequest<BaseResponse<VerifyEmailResponse>>;

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
            : IRequestHandler<VerifyEmailCommand, BaseResponse<VerifyEmailResponse>>
        {
            public async Task<BaseResponse<VerifyEmailResponse>> Handle(
                VerifyEmailCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var user = await userRepository.GetAsync(request.Email);
                    if (user is null)
                        return BaseResponse<VerifyEmailResponse>.Failure("User not found.");

                    if (user.IsEmailVerified)
                        return BaseResponse<VerifyEmailResponse>.Failure("Email is already verified.");

                    if (user.VerificationToken != request.Token)
                        return BaseResponse<VerifyEmailResponse>.Failure("Invalid verification token.");

                    if (user.VerificationTokenExpiryTime < DateTime.UtcNow)
                        return BaseResponse<VerifyEmailResponse>.Failure(
                            "Verification token has expired. Please request a new one.");

                    user.IsEmailVerified = true;
                    user.VerificationToken = null;
                    user.VerificationTokenExpiryTime = null;
                    user.DateModified = DateTime.UtcNow;

                    userRepository.Update(user);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<VerifyEmailResponse>.Success(
                        "Email verified successfully. You can now log in.",
                        new VerifyEmailResponse(user.Id.ToString()));
                }
                catch (Exception ex)
                {
                    return BaseResponse<VerifyEmailResponse>.Failure(
                        $"An error occurred during email verification: {ex.Message}");
                }
            }
            public record VerifyEmailResponse(string Email);
        }
    }
}