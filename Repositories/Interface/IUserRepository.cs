using MyApi.Models;

namespace MyApi.Repositories.Interface;

public interface IUserRepository
{
    Task<List<User>> GetAll();
    Task<User?> GetById(Guid id);
    Task<User> Create(User user);
    Task Update(User user);
    Task Delete(User user);
}