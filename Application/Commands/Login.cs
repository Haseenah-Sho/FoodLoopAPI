using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Commands
{
    public class Login
    {
        public record LoginCommand(
            string Email,
            string Password) : IRequest<BaseResponse<LoginResponse>>;

        public class LoginValidator : AbstractValidator<LoginCommand>
        {
            public LoginValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("A valid email address is required.");

                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required.");
            }
        }

        public class LoginHandler(
            IUserRepository userRepository,
            IJwtService jwtService,
            IPasswordHasher<User> passwordHasher)
            : IRequestHandler<LoginCommand, BaseResponse<LoginResponse>>
        {
            public async Task<BaseResponse<LoginResponse>> Handle(
                LoginCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var user = await userRepository.GetAsync(request.Email);
                    if (user is null)
                        return BaseResponse<LoginResponse>.Failure(
                            "Invalid email or password.");

                    if (!user.IsEmailVerified)
                        return BaseResponse<LoginResponse>.Failure(
                            "Your email address has not been verified. Please check your inbox.");

                    var passwordResult = passwordHasher.VerifyHashedPassword(
                        user, user.HashPassword, request.Password);

                    if (passwordResult == PasswordVerificationResult.Failed)
                        return BaseResponse<LoginResponse>.Failure(
                            "Invalid email or password.");

                    var roles = user.UserRoles
                        .Select(ur => ur.Role.Name)
                        .ToList();

                    var token = jwtService.GenerateToken(user, roles);

                    return BaseResponse<LoginResponse>.Success(
                        "Login successful.",
                        new LoginResponse(
                            user.Id,
                            user.FullName ?? user.UserName,
                            user.Email,
                            roles,
                            token));
                }
                catch (Exception ex)
                {
                    return BaseResponse<LoginResponse>.Failure(
                        $"An error occurred during login: {ex.Message}");
                }
            }
        }

        public record LoginResponse(
            Guid UserId,
            string Name,
            string Email,
            IList<string> Roles,
            string Token);
    }
}