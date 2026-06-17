using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using FluentValidation;
using MediatR;
using static Application.Commands.ResendVerificationToken.ResendVerificationTokenHandler;

namespace Application.Commands
{
    public class ResendVerificationToken
    {
        public record ResendVerificationTokenCommand(string Email) : IRequest<BaseResponse<ResendVerificationTokenResponse>>;

        public class ResendVerificationTokenValidator : AbstractValidator<ResendVerificationTokenCommand>
        {
            public ResendVerificationTokenValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("A valid email address is required.");
            }
        }

        public class ResendVerificationTokenHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
            : IRequestHandler<ResendVerificationTokenCommand, BaseResponse<ResendVerificationTokenResponse>>
        {
            public async Task<BaseResponse<ResendVerificationTokenResponse>> Handle(
                ResendVerificationTokenCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var user = await userRepository.GetAsync(request.Email);
                    if (user is null)
                        return BaseResponse<ResendVerificationTokenResponse>.Failure("User not found.");

                    if (user.IsEmailVerified)
                        return BaseResponse<ResendVerificationTokenResponse>.Failure("Email is already verified.");

                    string verificationToken = new Random().Next(1000, 9999).ToString();

                    user.VerificationToken = verificationToken;
                    user.VerificationTokenExpiryTime = DateTime.UtcNow.AddHours(24);
                    user.DateModified = DateTime.UtcNow;

                    userRepository.Update(user);
                    await unitOfWork.SaveAsync();

                    try
                    {
                        await emailService.SendVerificationEmailAsync(
                            request.Email, verificationToken);
                    }
                    catch (Exception)
                    {
                        // Email not sent, but token was saved in db
                    }

                    return BaseResponse<ResendVerificationTokenResponse>.Success(
                        "Verification token resent successfully. Please check your email.",
                        new ResendVerificationTokenResponse(user.Email));
                }
                catch (Exception ex)
                {
                    return BaseResponse<ResendVerificationTokenResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
            public record ResendVerificationTokenResponse(string Email);
        }
    }
}