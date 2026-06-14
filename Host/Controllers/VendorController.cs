using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.Vendors.Commands.ApproveRejectVendor;
using static Application.Features.Vendors.Commands.RegisterVendor;
using static Application.Features.Vendors.Queries.GetPendingVendors;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController(IMediator mediator) : ControllerBase
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
    }
}