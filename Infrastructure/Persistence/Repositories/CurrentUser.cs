using Application.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class CurrentUser(IHttpContextAccessor context) : ICurrentUser
{
    public Guid GetCurrentUser()
    {
        var sub = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(sub);
    }
}