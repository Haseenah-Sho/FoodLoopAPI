using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static Application.Commands.ResetPassword.ResetPasswordHandler;

namespace Application.Commands
{
    public class ResetPassword
    {
        public record ResetPasswordCommand(
            string Email,
            string Token,
            string NewPassword,
            string ConfirmPassword) : IRequest<BaseResponse<ResetPasswordResponse>>;

        public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
        {
            public ResetPasswordValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("A valid email address is required.");

                RuleFor(x => x.Token)
                    .NotEmpty().WithMessage("Reset token is required.");

                RuleFor(x => x.NewPassword)
                    .NotEmpty().WithMessage("New password is required.")
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

                RuleFor(x => x.ConfirmPassword)
                    .NotEmpty().WithMessage("Confirm password is required.")
                    .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
            }
        }

        public class ResetPasswordHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher<User> passwordHasher)
            : IRequestHandler<ResetPasswordCommand, BaseResponse<ResetPasswordResponse>>
        {
            public async Task<BaseResponse<ResetPasswordResponse>> Handle(
                ResetPasswordCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var user = await userRepository.GetAsync(request.Email);
                    if (user is null)
                        return BaseResponse<ResetPasswordResponse>.Failure("User not found.");

                    if (user.VerificationToken != request.Token)
                        return BaseResponse<ResetPasswordResponse>.Failure("Invalid reset token.");

                    if (user.VerificationTokenExpiryTime < DateTime.UtcNow)
                        return BaseResponse<ResetPasswordResponse>.Failure(
                            "Reset token has expired. Please request a new one.");

                    user.HashPassword = passwordHasher.HashPassword(user, request.NewPassword);
                    user.VerificationToken = null;
                    user.VerificationTokenExpiryTime = null;
                    user.DateModified = DateTime.UtcNow;

                    userRepository.Update(user);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<ResetPasswordResponse>.Success(
                        "Password reset successfully. You can now log in.",
                        new ResetPasswordResponse(user.Email));
                }
                catch (Exception ex)
                {
                    return BaseResponse<ResetPasswordResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
            public record ResetPasswordResponse(string Email);
        }
    }
}