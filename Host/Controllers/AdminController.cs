using Application.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Queries.GetAllCustomers;
using static Application.Queries.GetAllListings;
using static Application.Queries.GetAllVendors;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController(IMediator mediator) : ControllerBase
    {
        [HttpGet("vendors")]
        public async Task<IActionResult> GetAllVendors()
        {
            var result = await mediator.Send(new GetAllVendorsQuery());
            return Ok(result);
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var result = await mediator.Send(new GetAllCustomersQuery());
            return Ok(result);
        }

        [HttpGet("listings")]
        public async Task<IActionResult> GetAllListings()
        {
            var result = await mediator.Send(new GetAllListingsQuery());
            return Ok(result);
        }
    }
}