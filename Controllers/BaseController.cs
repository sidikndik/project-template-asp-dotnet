using Microsoft.AspNetCore.Mvc;
using MyApi.DTOs;

namespace MyApi.Controllers
{
    public class BaseController : ControllerBase
    {
        protected IActionResult Success<T>(T data, string message = "Success")
        {
            return Ok(ApiResponse<T>.SuccessResponse(data, message));
        }

        protected IActionResult Created<T>(T data, string message = "Created")
        {
            return StatusCode(201, ApiResponse<T>.SuccessResponse(data, message));
        }

        protected IActionResult BadRequestResponse(string message = "Bad Request", object? errors = null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message, errors));
        }

        protected IActionResult NotFoundResponse(string message = "Data not found")
        {
            return NotFound(ApiResponse<object>.ErrorResponse(message));
        }

        protected IActionResult UnauthorizedResponse(string message = "Unauthorized")
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(message));
        }

        protected IActionResult ForbiddenResponse(string message = "Forbidden")
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(message));
        }

        protected IActionResult ServerErrorResponse(string message = "Internal Server Error")
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(message));
        }
    }
}