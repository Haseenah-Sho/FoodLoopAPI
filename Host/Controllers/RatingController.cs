using Application.Constants;
using Application.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.AddRating;
using static Application.Queries.GetRating;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> AddRating([FromBody] AddRatingRequest request)
        {
            var customerUserId = currentUser.GetCurrentUser();
            var command = new AddRatingCommand(customerUserId, request.ListingId, request.Stars, request.Comment);
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{listingId}")]
        public async Task<IActionResult> GetListingRatings(Guid listingId)
        {
            var result = await mediator.Send(new GetRatingQuery(listingId));
            return Ok(result);
        }
    }

    public record AddRatingRequest(Guid ListingId, int Stars, string? Comment);
}