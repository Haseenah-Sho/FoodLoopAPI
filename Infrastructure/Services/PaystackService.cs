using Application.Common.Models;
using Application.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class PaystackService(HttpClient httpClient, IOptions<PaystackSettings> options) : IPaystackService
    {
        private readonly PaystackSettings _settings = options.Value;

        private HttpRequestMessage BuildRequest(HttpMethod method, string path)
        {
            var request = new HttpRequestMessage(method, $"{_settings.BaseUrl}{path}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
            return request;
        }

        public async Task<InitializePaymentResult> InitializeTransactionAsync(
            string email, decimal amount, string reference, string? subaccountCode = null)
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    ["email"] = email,
                    ["amount"] = (int)(amount * 100), // kobo
                    ["reference"] = reference,
                    ["callback_url"] = _settings.CallbackUrl
                };

                if (!string.IsNullOrWhiteSpace(subaccountCode))
                {
                    payload["subaccount"] = subaccountCode;
                    payload["bearer"] = "subaccount";
                }

                var request = BuildRequest(HttpMethod.Post, "/transaction/initialize");
                request.Content = JsonContent.Create(payload);

                var response = await httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new InitializePaymentResult(false, null, null, $"Paystack error: {json}");

                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                var authorizationUrl = data.GetProperty("authorization_url").GetString();
                var refFromPaystack = data.GetProperty("reference").GetString();

                return new InitializePaymentResult(true, authorizationUrl, refFromPaystack, null);
            }
            catch (Exception ex)
            {
                return new InitializePaymentResult(false, null, null, ex.Message);
            }
        }

        public async Task<VerifyPaymentResult> VerifyTransactionAsync(string reference)
        {
            try
            {
                var request = BuildRequest(HttpMethod.Get, $"/transaction/verify/{reference}");
                var response = await httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new VerifyPaymentResult(false, false, 0, $"Paystack error: {json}");

                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                var status = data.GetProperty("status").GetString();
                var amountInKobo = data.GetProperty("amount").GetInt64();

                bool isPaid = status == "success";
                decimal amountPaid = amountInKobo / 100m;

                return new VerifyPaymentResult(true, isPaid, amountPaid, null);
            }
            catch (Exception ex)
            {
                return new VerifyPaymentResult(false, false, 0, ex.Message);
            }
        }

        public async Task<ResolveAccountResult> ResolveAccountNumberAsync(string accountNumber, string bankCode)
        {
            try
            {
                var request = BuildRequest(HttpMethod.Get, $"/bank/resolve?account_number={accountNumber}&bank_code={bankCode}");
                var response = await httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = "Could not verify that account number.";
                    try
                    {
                        using var errDoc = JsonDocument.Parse(json);
                        if (errDoc.RootElement.TryGetProperty("message", out var msgProp))
                            errorMessage = msgProp.GetString() ?? errorMessage;
                    }
                    catch { /* fall back to generic message */ }

                    return new ResolveAccountResult(false, null, errorMessage);
                }

                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                var accountName = data.GetProperty("account_name").GetString();

                return new ResolveAccountResult(true, accountName, null);
            }
            catch (Exception ex)
            {
                return new ResolveAccountResult(false, null, ex.Message);
            }
        }

        public async Task<CreateSubaccountResult> CreateSubaccountAsync(
            string businessName, string bankCode, string accountNumber, decimal percentageCharge)
        {
            try
            {
                var payload = new
                {
                    business_name = businessName,
                    bank_code = bankCode,
                    account_number = accountNumber,
                    percentage_charge = (double)percentageCharge
                };

                var request = BuildRequest(HttpMethod.Post, "/subaccount");
                request.Content = JsonContent.Create(payload);

                var response = await httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new CreateSubaccountResult(false, null, $"Paystack error: {json}");

                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                var subaccountCode = data.GetProperty("subaccount_code").GetString();

                return new CreateSubaccountResult(true, subaccountCode, null);
            }
            catch (Exception ex)
            {
                return new CreateSubaccountResult(false, null, ex.Message);
            }
        }

        public async Task<ListBanksResult> ListBanksAsync()
        {
            try
            {
                var request = BuildRequest(HttpMethod.Get, "/bank?country=nigeria");
                var response = await httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new ListBanksResult(false, new List<BankInfo>(), $"Paystack error: {json}");

                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");

                var banks = data.EnumerateArray()
                    .Select(b => new BankInfo(
                        b.GetProperty("name").GetString()!,
                        b.GetProperty("code").GetString()!))
                    .ToList();

                return new ListBanksResult(true, banks, null);
            }
            catch (Exception ex)
            {
                return new ListBanksResult(false, new List<BankInfo>(), ex.Message);
            }
        }
    }
}