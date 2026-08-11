using Application.Common.Dtos;
using Application.Common.Models;
using Application.Repositories;
using Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Commands
{
    public class SetVendorBankDetails
    {
        public record SetVendorBankDetailsCommand(
            Guid UserId,
            string BankCode,
            string BankName,
            string AccountNumber) : IRequest<BaseResponse<SetVendorBankDetailsResponse>>;

        public class SetVendorBankDetailsValidator : AbstractValidator<SetVendorBankDetailsCommand>
        {
            public SetVendorBankDetailsValidator()
            {
                RuleFor(x => x.BankCode).NotEmpty().WithMessage("Bank is required.");
                RuleFor(x => x.BankName).NotEmpty().WithMessage("Bank is required.");
                RuleFor(x => x.AccountNumber)
                    .NotEmpty().WithMessage("Account number is required.")
                    .Matches(@"^\d{10}$").WithMessage("Account number must be 10 digits.");
            }
        }

        public class SetVendorBankDetailsHandler(
            IVendorRepository vendorRepository,
            IUnitOfWork unitOfWork,
            IPaystackService paystackService,
            IOptions<PaystackSettings> paystackSettings)
            : IRequestHandler<SetVendorBankDetailsCommand, BaseResponse<SetVendorBankDetailsResponse>>
        {
            public async Task<BaseResponse<SetVendorBankDetailsResponse>> Handle(
                SetVendorBankDetailsCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.UserId);
                    if (vendor is null)
                        return BaseResponse<SetVendorBankDetailsResponse>.Failure("Vendor profile not found.");

                    var resolveResult = await paystackService.ResolveAccountNumberAsync(
                        request.AccountNumber, request.BankCode);

                    if (!resolveResult.Success || string.IsNullOrWhiteSpace(resolveResult.AccountName))
                        return BaseResponse<SetVendorBankDetailsResponse>.Failure(
                            resolveResult.ErrorMessage ?? "Could not verify this account number.");

                    var subaccountResult = await paystackService.CreateSubaccountAsync(
                        vendor.OrganizationName,
                        request.BankCode,
                        request.AccountNumber,
                        paystackSettings.Value.PlatformCommissionPercentage);

                    if (!subaccountResult.Success || string.IsNullOrWhiteSpace(subaccountResult.SubaccountCode))
                        return BaseResponse<SetVendorBankDetailsResponse>.Failure(
                            subaccountResult.ErrorMessage ?? "Could not set up payouts for this account.");

                    vendor.BankCode = request.BankCode;
                    vendor.BankName = request.BankName;
                    vendor.BankAccountNumber = request.AccountNumber;
                    vendor.AccountName = resolveResult.AccountName;
                    vendor.PaystackSubaccountCode = subaccountResult.SubaccountCode;
                    vendor.DateModified = DateTime.UtcNow;

                    vendorRepository.Update(vendor);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<SetVendorBankDetailsResponse>.Success(
                        "Payout account linked. Paid orders will now settle directly to this account.",
                        new SetVendorBankDetailsResponse(vendor.BankName!, vendor.BankAccountNumber!, vendor.AccountName!));
                }
                catch (Exception ex)
                {
                    return BaseResponse<SetVendorBankDetailsResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record SetVendorBankDetailsResponse(string BankName, string AccountNumber, string AccountName);
    }
}