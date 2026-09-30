using Application.Common.Dtos;
using Application.Constants;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands
{
    public class RegisterCustomer
    {
        public record RegisterCustomerCommand(
            string FullName,
            string UserName,
            string PhoneNumber,
            string Address,
            string Email,
            string Password,
            string ConfirmPassword) : IRequest<BaseResponse<RegisterCustomerResponse>>;

        public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerCommand>
        {
            public RegisterCustomerValidator()
            {
                RuleFor(x => x.FullName)
                    .NotEmpty().WithMessage("Full name is required.")
                    .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

                RuleFor(x => x.UserName)
                    .NotEmpty().WithMessage("Username is required.")
                    .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.");

                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required.")
                    .Matches(@"^(\+?\d{1,3}[-\s]?)?0?\d{10}$")
                    .WithMessage("Enter a valid phone number.");

                RuleFor(x => x.Address)
                    .NotEmpty().WithMessage("Address is required.")
                    .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");

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

        public class RegisterCustomerHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher<User> passwordHasher,
            IEmailService emailService)
            : IRequestHandler<RegisterCustomerCommand, BaseResponse<RegisterCustomerResponse>>
        {
            public async Task<BaseResponse<RegisterCustomerResponse>> Handle(
                RegisterCustomerCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customerRole = await roleRepository.GetAsync(AppRoles.Customer);
                    if (customerRole is null)
                        return BaseResponse<RegisterCustomerResponse>.Failure(
                            "Customer role not found. Please contact support.");

                    var existingUser = await userRepository.GetAsync(request.Email);

                    if (existingUser is not null)
                    {
                        var alreadyCustomer = await customerRepository
                            .GetCustomerByUserIdAsync(existingUser.Id);

                        if (alreadyCustomer is not null)
                            return BaseResponse<RegisterCustomerResponse>.Failure(
                                "A customer profile already exists for this email.");

                 
                        var alreadyHasRole = await userRoleRepository
                            .IsExistAsync(existingUser.Id, customerRole.Id);

                        if (!alreadyHasRole)
                        {
                            await userRoleRepository.AddAsync(new UserRole
                            {
                                UserId = existingUser.Id,
                                RoleId = customerRole.Id
                            });
                        }

                        var customer = new Customer
                        {
                            UserId = existingUser.Id,
                            PhoneNumber = request.PhoneNumber,
                            Address = request.Address,
                            CreatedBy = existingUser.Email,
                        };
                        await customerRepository.AddAsync(customer);
                        await unitOfWork.SaveAsync();

                        return BaseResponse<RegisterCustomerResponse>.Success(
                            "Customer profile created successfully.",
                            new RegisterCustomerResponse(existingUser.Id, existingUser.Email));
                    }

                    string verificationToken = new Random().Next(100000, 999999).ToString();

                    var user = new User
                    {
                        FullName = request.FullName,
                        UserName = request.UserName,
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
                        RoleId = customerRole.Id
                    });

                    await customerRepository.AddAsync(new Customer
                    {
                        UserId = user.Id,
                        PhoneNumber = request.PhoneNumber,
                        Address = request.Address,
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
                        // Email failed but registration successful
                    }

                    return BaseResponse<RegisterCustomerResponse>.Success(
                        "Registration successful. Please check your email to verify your account.",
                        new RegisterCustomerResponse(user.Id, user.Email));
                }
                catch (Exception ex)
                {
                    return BaseResponse<RegisterCustomerResponse>.Failure(
                        $"An error occurred during registration: {ex.Message}");
                }
            }
        }

        public record RegisterCustomerResponse(Guid UserId, string Email);
    }
}