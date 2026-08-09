using Application.Constants;
using Application.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CancelOrder;
using static Application.Commands.DispatchOrder;
using static Application.Commands.MarkAsDelivered;
using static Application.Commands.PlaceOrder;
using static Application.Commands.ResumePayment;
using static Application.Commands.VerifyPickup;
using static Application.Queries.GetOrderByOrderNo;
using static Application.Queries.GetOrderDetails;
using static Application.Queries.GetOrders;
using static Application.Queries.GetVendorOrders;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> Create([FromBody] PlaceOrderRequest request)
        {
            var customerUserId = currentUser.GetCurrentUser();

            var command = new PlaceOrderCommand(
                customerUserId,
                request.Items,
                request.FulfilmentType,
                request.DeliveryAddress);

            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetMyOrders()
        {
            var customerUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetOrdersQuery(customerUserId));
            return Ok(result);
        }

        [HttpGet("vendor-orders")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> GetVendorOrders()
        {
            var vendorUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetVendorOrdersQuery(vendorUserId));
            return Ok(result);
        }

        [HttpGet("order-item/{orderNo}")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> GetOrderItem(string orderNo)
        {
            var vendorUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetOrderByOrderNoQuery(vendorUserId, orderNo));
            return Ok(result);
        }

        [HttpGet("{orderId}")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetOrderDetails(Guid orderId)
        {
            var customerUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetOrderDetailsQuery(customerUserId, orderId));
            return Ok(result);
        }

        [HttpPut("verify-pickup/{orderNo}")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> VerifyPickup(string orderNo)
        {
            var vendorUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new VerifyPickupCommand(vendorUserId, orderNo));
            return Ok(result);
        }

        [HttpPut("dispatch-order/{orderNo}")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> DispatchDelivery(string orderNo)
        {
            var vendorUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new DispatchOrderCommand(vendorUserId, orderNo));
            return Ok(result);
        }

        [HttpPut("mark-delivered/{orderNo}")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> MarkDelivered(string orderNo)
        {
            var vendorUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new MarkAsDeliveredCommand(vendorUserId, orderNo));
            return Ok(result);
        }

        [HttpDelete("{orderId}")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            var customerUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new CancelOrderCommand(customerUserId, orderId));
            return Ok(result);
        }

        [HttpPost("{orderId}/resume-payment")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> ResumePayment(Guid orderId)
        {
            var customerUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new ResumePaymentCommand(customerUserId, orderId));
            return Ok(result);
        }

    }

    public record PlaceOrderRequest(
        List<PlaceOrderItemRequest> Items,
        Domain.Enums.FulfilmentType FulfilmentType,
        string? DeliveryAddress);
}