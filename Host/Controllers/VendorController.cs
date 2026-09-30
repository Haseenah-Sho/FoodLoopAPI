using Application.Constants;
using Application.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.ApproveRejectVendor;
using static Application.Commands.RegisterVendor;
using static Application.Commands.SetVendorBankDetails;
using static Application.Commands.UpdateVendorProfile;
using static Application.Queries.GetBanks;
using static Application.Queries.GetPendingVendors;
using static Application.Queries.GetVendorProfile;
using static Application.Queries.ResolveVendorAccount;
using static Application.Commands.ManageDeliveryZones;
using static Application.Queries.GetVendorZones;
using static Application.Commands.ManagePickupPoints;
using static Application.Queries.GetVendorPickupPoints;
using static Application.Queries.GetPublicDeliveryZones;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterVendorCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("approve-reject")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> ApproveReject([FromBody] ApproveRejectVendorCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("pending")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> GetPending()
        {
            var result = await mediator.Send(new GetPendingVendorsQuery());
            return Ok(result);
        }

        [HttpGet("profile")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetVendorProfileQuery(userId));
            return Ok(result);
        }

        [HttpPut("profile")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateVendorProfileRequest request)
        {
            var userId = currentUser.GetCurrentUser();
            var command = new UpdateVendorProfileCommand(
                userId, request.OrganizationName, request.PhoneNumber, request.Address);
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("banks")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> GetBanks()
        {
            var result = await mediator.Send(new GetBanksQuery());
            return Ok(result);
        }

        [HttpGet("resolve-account")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> ResolveAccount([FromQuery] string accountNumber, [FromQuery] string bankCode)
        {
            var result = await mediator.Send(new ResolveVendorAccountQuery(accountNumber, bankCode));
            return Ok(result);
        }

        [HttpPost("bank-details")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> SetBankDetails([FromBody] SetVendorBankDetailsRequest request)
        {
            var userId = currentUser.GetCurrentUser();
            var command = new SetVendorBankDetailsCommand(userId, request.BankCode, request.BankName, request.AccountNumber);
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("delivery-zones")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> GetDeliveryZones()
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetVendorZonesQuery(userId));
            return Ok(result);
        }

        [HttpPost("delivery-zones")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> AddDeliveryZone([FromBody] AddZoneRequest request)
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new AddDeliveryZoneCommand(userId, request.ZoneName, request.Fee));
            return Ok(result);
        }

        [HttpDelete("delivery-zones/{zoneId}")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> RemoveDeliveryZone(Guid zoneId)
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new RemoveDeliveryZoneCommand(userId, zoneId));
            return Ok(result);
        }

        [HttpGet("pickup-points")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> GetPickupPoints()
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetVendorPickupPointsQuery(userId));
            return Ok(result);
        }

        [HttpPost("pickup-points")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> AddPickupPoint([FromBody] AddPointRequest request)
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new AddPickupPointCommand(userId, request.PointName, request.Address));
            return Ok(result);
        }

        [HttpDelete("pickup-points/{pointId}")]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> RemovePickupPoint(Guid pointId)
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new RemovePickupPointCommand(userId, pointId));
            return Ok(result);
        }

        [HttpGet("{vendorId}/delivery-zones-public")]
        public async Task<IActionResult> GetPublicDeliveryZones(Guid vendorId)
        {
            var result = await mediator.Send(new GetPublicDeliveryZonesQuery(vendorId));
            return Ok(result);
        }


        public record UpdateVendorProfileRequest(string OrganizationName, string PhoneNumber, string Address);
        public record SetVendorBankDetailsRequest(string BankCode, string BankName, string AccountNumber);
        public record AddZoneRequest(string ZoneName, decimal Fee);
        public record AddPointRequest(string PointName, string Address);
    }
}