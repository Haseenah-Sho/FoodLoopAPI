using Application.Constants;
using Application.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.UpdateCustomerProfile;
using static Application.Queries.GetCustomerProfile;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
    {
        [HttpGet("profile")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetCustomerProfileQuery(userId));
            return Ok(result);
        }

        [HttpPut("profile")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileRequest request)
        {
            var userId = currentUser.GetCurrentUser();
            var command = new UpdateCustomerProfileCommand(userId, request.PhoneNumber, request.Address);
            var result = await mediator.Send(command);
            return Ok(result);
        }

        public record UpdateCustomerProfileRequest(string PhoneNumber, string Address);
    }
}