using MyApi.Models;
using MyApi.DTOs;
using MyApi.Services.Interface;
using MyApi.Repositories.Interface;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repo, ILogger<UserService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<User>> GetAll()
    {
        _logger.LogInformation("Fetching all users");
        return await _repo.GetAll();
    }

    public async Task<User?> GetById(Guid id)
    {
        _logger.LogInformation("Fetching users by id");
        return await _repo.GetById(id);
    }

    public async Task<User> Create(CreateUserDto dto)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email
        };

        return await _repo.Create(user);
    }

    public async Task<bool> Update(Guid id, UpdateUserDto dto)
    {
        var user = await _repo.GetById(id);
        if (user == null) return false;

        user.Name = dto.Name;
        user.Email = dto.Email;

        await _repo.Update(user);
        return true;
    }

    public async Task<bool> Delete(Guid id)
    {
        var user = await _repo.GetById(id);
        if (user == null) return false;

        await _repo.Delete(user);
        return true;
    }
}