using Application.Common.Dtos;
using Application.Constants;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static Application.Commands.RegisterVendor.RegisterVendorHandler;

namespace Application.Commands
{
    public class RegisterVendor
    {
        public record RegisterVendorCommand(
            string FullName,
            string OrganizationName,
            string PhoneNumber,
            string Email,
            string Password,
            string ConfirmPassword) : IRequest<BaseResponse<RegisterVendorResponse>>;

        public class RegisterVendorValidator : AbstractValidator<RegisterVendorCommand>
        {
            public RegisterVendorValidator()
            {
                RuleFor(x => x.OrganizationName)
                    .NotEmpty().WithMessage("Organization name is required.")
                    .MaximumLength(200).WithMessage("Organization name cannot exceed 200 characters.");

                RuleFor(x => x.PhoneNumber)
                    .NotEmpty()
                    .WithMessage("Enter phone number")
                    .Matches(@"^\+?[1-9]\d{1,14}$")
                    .WithMessage("Enter a valid phoneNumber");

                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("A valid email address is required.");

                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required.")
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

                RuleFor(x => x.ConfirmPassword)
                    .NotEmpty().WithMessage("Confirm password is required.")
                    .Equal(x => x.Password).WithMessage("Passwords do not match.");
            }
        }

        public class RegisterVendorHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IVendorRepository vendorRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher<User> passwordHasher,
            IEmailService emailService)
            : IRequestHandler<RegisterVendorCommand, BaseResponse<RegisterVendorResponse>>
        {
            public async Task<BaseResponse<RegisterVendorResponse>> Handle(
    RegisterVendorCommand request,
    CancellationToken cancellationToken)
            {
                try
                {
                    var vendorRole = await roleRepository.GetAsync(AppRoles.Vendor);
                    if (vendorRole is null)
                        return BaseResponse<RegisterVendorResponse>.Failure(
                            "Vendor role not found. Please contact support.");

                    string verificationToken = new Random().Next(100000, 999999).ToString();

                    var userExists = await userRepository.GetAsync(request.Email);

                    if (userExists is not null)
                    {
                        var alreadyVendor = await vendorRepository.IsExistAsync(userExists.Id);
                        if (alreadyVendor)
                            return BaseResponse<RegisterVendorResponse>.Failure(
                                "A vendor profile already exists for this email.");

                        var alreadyHasRole = await userRoleRepository.IsExistAsync(
                            userExists.Id, vendorRole.Id);
                        if (!alreadyHasRole)
                        {
                            await userRoleRepository.AddAsync(new UserRole
                            {
                                UserId = userExists.Id,
                                RoleId = vendorRole.Id
                            });
                        }

                        var vendor = new Vendor
                        {
                            UserId = userExists.Id,
                            OrganizationName = request.OrganizationName,
                            PhoneNumber = request.PhoneNumber,
                            IsApproved = false,
                            CreatedBy = request.Email,
                        };
                        await vendorRepository.AddAsync(vendor);
                        await unitOfWork.SaveAsync();

                        return BaseResponse<RegisterVendorResponse>.Success(
                            "Vendor profile created. Pending admin approval.",
                            new RegisterVendorResponse(userExists.Id, userExists.Email));
                    }

                    var user = new User
                    {
                        FullName = request.FullName,
                        UserName = request.Email,
                        Email = request.Email,
                        IsEmailVerified = false,
                        VerificationToken = verificationToken,
                        VerificationTokenExpiryTime = DateTime.UtcNow.AddHours(24),
                        CreatedBy = request.Email,
                    };
                    user.HashPassword = passwordHasher.HashPassword(user, request.Password);

                    await userRepository.AddAsync(user);

                    await userRoleRepository.AddAsync(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = vendorRole.Id
                    });

                    await vendorRepository.AddAsync(new Vendor
                    {
                        UserId = user.Id,
                        OrganizationName = request.OrganizationName,
                        PhoneNumber = request.PhoneNumber,
                        IsApproved = false,
                        CreatedBy = request.Email,
                    });

                    await unitOfWork.SaveAsync();

                    try
                    {
                        await emailService.SendVerificationEmailAsync(
                            request.Email, verificationToken);
                    }
                    catch (Exception)
                    {
                        // cannot send email but registration succeeded
                    }

                    return BaseResponse<RegisterVendorResponse>.Success(
                        "Registration successful. Your account is pending admin approval.",
                        new RegisterVendorResponse(user.Id, user.Email));
                }
                catch (Exception ex)
                {
                    return BaseResponse<RegisterVendorResponse>.Failure(
                        $"An error occurred during registration: {ex.Message}");
                }
            }
            public record RegisterVendorResponse(Guid UserId, string Email);
        }
    }
}