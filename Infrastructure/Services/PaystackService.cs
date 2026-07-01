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

        public async Task<InitializePaymentResult> InitializeTransactionAsync(string email, decimal amount, string reference)
        {
            try
            {
                httpClient.BaseAddress = new Uri(_settings.BaseUrl);
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _settings.SecretKey);

                var payload = new
                {
                    email,
                    amount = (int)(amount * 100), // Paystack expects amount in kobo
                    reference
                };

                var response = await httpClient.PostAsJsonAsync("/transaction/initialize", payload);
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
                httpClient.BaseAddress = new Uri(_settings.BaseUrl);
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _settings.SecretKey);

                var response = await httpClient.GetAsync($"/transaction/verify/{reference}");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new VerifyPaymentResult(false, false, 0, $"Paystack error: {json}");

                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                var status = data.GetProperty("status").GetString(); // "success", "failed", "abandoned"
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
    }
}