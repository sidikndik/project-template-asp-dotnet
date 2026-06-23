using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.DTOs;
using MyApi.Services.Interface;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(IConfiguration configuration, IJwtTokenService jwtTokenService)
        {
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto dto)
        {
            var demoUsername = _configuration["DemoAuth:Username"];
            var demoPassword = _configuration["DemoAuth:Password"];

            if (dto.Username != demoUsername || dto.Password != demoPassword)
                return UnauthorizedResponse("Invalid username or password");

            var token = _jwtTokenService.GenerateToken(dto.Username!, new[] { "Admin" });
            return Success(token, "Login success");
        }
    }
}
