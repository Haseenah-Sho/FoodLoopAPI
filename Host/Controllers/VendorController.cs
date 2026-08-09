using Application.Constants;
using Application.Queries;
using Application.Repositories;
using Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.ApproveRejectVendor;
using static Application.Commands.RegisterVendor;
using static Application.Commands.UpdateVendorProfile;
using static Application.Queries.GetAllVendors;
using static Application.Queries.GetListings;
using static Application.Queries.GetPendingVendors;
using static Application.Queries.GetVendorProfile;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterVendorCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }
         
        [HttpPut("approve-reject")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> ApproveReject(
            [FromBody] ApproveRejectVendorCommand command)
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
            var command = new UpdateVendorProfileCommand(userId, request.OrganizationName, request.PhoneNumber);
            var result = await mediator.Send(command);
            return Ok(result);
        }


        public record UpdateVendorProfileRequest(string OrganizationName, string PhoneNumber);
    }
}