using MyApi.Models;
using MyApi.DTOs;

namespace MyApi.Services.Interface;

public interface IUserService
{
    Task<List<User>> GetAll();
    Task<PagedResult<User>> GetPaged(QueryParameters parameters);
    Task<User?> GetById(Guid id);
    Task<User> Create(CreateUserDto dto);
    Task<bool> Update(Guid id, UpdateUserDto dto);
    Task<bool> Delete(Guid id);
}
