using Application.Constants;
using Application.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.VerifyPayment;
using static Application.Queries.GetCustomerTransactionHistory;
using static Application.Queries.GetVendorTransactionHistory;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost("verify")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> Verify([FromBody] VerifyPaymentCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
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
    }
}