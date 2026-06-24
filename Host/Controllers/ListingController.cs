using Application.Constants;
using Application.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CreateListing;
using static Application.Queries.GetListingDetails;
using static Application.Queries.GetListings;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = AppRoles.Vendor)]
        public async Task<IActionResult> Create([FromForm] CreateListingCommand command)
        {
            command.VendorUserId = currentUser.GetCurrentUser();
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetListings([FromQuery] bool? isFree)
        {
            var result = await mediator.Send(new GetListingsQuery(isFree));
            return Ok(result);
        }

        [HttpGet("{listingId}")]
        public async Task<IActionResult> GetListingDetails(Guid listingId)
        {
            var result = await mediator.Send(new GetListingDetailsQuery(listingId));
            return Ok(result);
        }
    }
}