using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.ForgotPassword;
using static Application.Commands.Login;
using static Application.Commands.RegisterCustomer;
using static Application.Commands.ResendVerificationToken;
using static Application.Commands.ResetPassword;
using static Application.Commands.VerifyEmail;

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

        [HttpPost("resend-verification-token")]
        public async Task<IActionResult> ResendVerificationToken(
            [FromBody] ResendVerificationTokenCommand command)
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }
    }
}