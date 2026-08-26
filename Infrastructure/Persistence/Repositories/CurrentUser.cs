using Application.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Persistence.Repositories
{
    public class CurrentUser(IHttpContextAccessor context) : ICurrentUser
    {
        public Guid GetCurrentUser()
        {
            var sub = context.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            return userId;
        }
    }
}