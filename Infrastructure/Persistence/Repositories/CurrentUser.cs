using Application.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class CurrentUser(IHttpContextAccessor context) : ICurrentUser
{
    public Guid GetCurrentUser()
    {
        var sub = context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub);
    }
}