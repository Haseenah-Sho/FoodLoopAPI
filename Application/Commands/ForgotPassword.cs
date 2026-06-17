using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using FluentValidation;
using MediatR;
using static Application.Commands.ForgotPassword.ForgotPasswordHandler;

namespace Application.Commands
{
    public class ForgotPassword
    {
        public record ForgotPasswordCommand(
            string Email) : IRequest<BaseResponse<ForgotPasswordResponse>>;

        public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
        {
            public ForgotPasswordValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("A valid email address is required.");
            }
        }

        public class ForgotPasswordHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
            : IRequestHandler<ForgotPasswordCommand, BaseResponse<ForgotPasswordResponse>>
        {
            public async Task<BaseResponse<ForgotPasswordResponse>> Handle(
                ForgotPasswordCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var user = await userRepository.GetAsync(request.Email);
                    if (user is null)
                        return BaseResponse<ForgotPasswordResponse>.Failure("User not found.");

                    if (!user.IsEmailVerified)
                        return BaseResponse<ForgotPasswordResponse>.Failure(
                            "Your email address has not been verified.");

                    string resetToken = new Random().Next(1000, 9999).ToString();

                    user.VerificationToken = resetToken;
                    user.VerificationTokenExpiryTime = DateTime.UtcNow.AddMinutes(45);
                    user.DateModified = DateTime.UtcNow;

                    userRepository.Update(user);
                    await unitOfWork.SaveAsync();

                    try
                    {
                        await emailService.SendPasswordResetEmailAsync(
                            request.Email, resetToken);
                    }
                    catch (Exception)
                    {
                        // Email not sent, but token was saved in db
                    }

                    return BaseResponse<ForgotPasswordResponse>.Success(
                        "Password reset code sent. Please check your email.",
                        new ForgotPasswordResponse(user.Email));
                }
                catch (Exception ex)
                {
                    return BaseResponse<ForgotPasswordResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
            public record ForgotPasswordResponse(string Email);
        }
    }
}