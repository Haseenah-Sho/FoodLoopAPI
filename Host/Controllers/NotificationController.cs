using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Repositories;
using static Application.Queries.GetMyNotifications;
using static Application.Commands.MarkNotificationRead;
using static Application.Commands.MarkAllNotificationsRead;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/notification")]
    [Authorize]
    public class NotificationController(
        IMediator mediator,
        ICurrentUser currentUser) : ControllerBase
    {
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new GetMyNotificationsQuery(userId));
            return Ok(result);
        }

        [HttpPut("mark-read/{notificationId}")]
        public async Task<IActionResult> MarkRead(Guid notificationId)
        {
            var result = await mediator.Send(new MarkNotificationReadCommand(notificationId));
            return Ok(result);
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = currentUser.GetCurrentUser();
            var result = await mediator.Send(new MarkAllNotificationsReadCommand(userId));
            return Ok(result);
        }
    }
}