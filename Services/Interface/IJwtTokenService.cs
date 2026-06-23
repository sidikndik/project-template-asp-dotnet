using MyApi.DTOs;

namespace MyApi.Services.Interface
{
    public interface IJwtTokenService
    {
        LoginResponseDto GenerateToken(string username, IEnumerable<string>? roles = null);
    }
}
