using Application.Common.Behaviours;
using Application.Common.Behaviours.ValidationBehaviour;
using Application.Common.Models;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static Application.Commands.RegisterVendor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderListingRepository, OrderListingRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IStrikeRepository, StrikeRepository>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }

    public static IServiceCollection AddAppSettings(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
        return services;
    }

    public static IServiceCollection AddMediatRWithBehaviours(
        this IServiceCollection services)
    {
        // Scans Application assembly for all IRequestHandler implementations
        services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(
        typeof(RegisterVendorCommand).Assembly));

        // Scans Application assembly for all AbstractValidator implementations
        services.AddValidatorsFromAssembly(typeof(RegisterVendorCommand).Assembly);

        // Runs validators automatically before every handler
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehaviour<,>));

        // Mapster
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(RegisterVendorCommand).Assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
    public static IServiceCollection AddJwtAuthentication(
    this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

        services.AddScoped<IJwtService, JwtService>();
        var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });

        return services;
    }

}