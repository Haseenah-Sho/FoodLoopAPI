using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.Auth.Commands.Login;
using static Application.Features.Customers.Commands.RegisterCustomer;
using static Application.Features.Customers.Commands.VerifyEmail;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCustomerCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(
            [FromBody] VerifyEmailCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }
    }
}