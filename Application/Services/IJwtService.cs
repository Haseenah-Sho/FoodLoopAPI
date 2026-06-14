using Domain.Entities;

namespace Application.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user, IList<string> roles);
    }
}