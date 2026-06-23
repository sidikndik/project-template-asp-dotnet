using Microsoft.AspNetCore.Mvc;
using MyApi.Controllers;
using MyApi.DTOs;
using MyApi.Services.Interface;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAll();
        return Success(data, "Sucess get data users");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _service.GetById(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        return Success(user, "Success get data user");
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        var user = await _service.Create(dto);
        return Created(user, "Sucess created");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto dto)
    {
        var result = await _service.Update(id, dto);
        if (!result) return NotFoundResponse("User not found");

        return Success("OK", "User Updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.Delete(id);
        if (!result) return NotFoundResponse("User not found");

        return Success("OK", "User Deleted");
    }
}