using Application.Common.Models;
using Application.Constants;
using Application.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static Application.Commands.VerifyPayment;
using static Application.Queries.GetCustomerTransactionHistory;
using static Application.Queries.GetVendorTransactionHistory;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(
        IMediator mediator,
        ICurrentUser currentUser,
        IOptions<PaystackSettings> paystackSettings,
        ILogger<PaymentController> logger) : ControllerBase
    {
        [HttpPost("verify")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> Verify([FromBody] VerifyPaymentCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            Request.EnableBuffering();
            string rawBody;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                rawBody = await reader.ReadToEndAsync();
            }
            Request.Body.Position = 0;

            if (!Request.Headers.TryGetValue("x-paystack-signature", out var signatureHeader))
            {
                logger.LogWarning("Paystack webhook received with no signature header.");
                return Ok();
            }

            var expectedSignature = ComputeSignature(rawBody, paystackSettings.Value.SecretKey);

            if (!string.Equals(expectedSignature, signatureHeader, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Paystack webhook signature mismatch — ignoring payload.");
                return Ok();
            }

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;
                var eventType = root.GetProperty("event").GetString();

                if (eventType == "charge.success")
                {
                    var reference = root.GetProperty("data").GetProperty("reference").GetString();
                    if (!string.IsNullOrWhiteSpace(reference))
                    {
                        await mediator.Send(new VerifyPaymentCommand(reference));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Paystack webhook payload.");
            }

            // Always 200 — Paystack retries on non-2xx, and we never want to leak
            // verification details back to the caller of this endpoint.
            return Ok();
        }

        [HttpGet("customer-payments-transactions")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetMyPayments()
        {
            var customerUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetCustomerTransactionHistoryQuery(customerUserId));
            return Ok(result);
        }

        [HttpGet("vendor-transactions")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> GetVendorTransactions()
        {
            var vendorUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetVendorTransactionHistoryQuery(vendorUserId));
            return Ok(result);
        }

        private static string ComputeSignature(string payload, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(payloadBytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}