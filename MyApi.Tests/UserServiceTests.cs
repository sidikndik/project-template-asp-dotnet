using Microsoft.Extensions.Logging.Abstractions;
using MyApi.DTOs;
using MyApi.Models;
using MyApi.Repositories.Interface;

namespace MyApi.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task Create_MapsDtoToUserAndSavesThroughRepository()
    {
        var repository = new FakeUserRepository();
        var service = new UserService(repository, NullLogger<UserService>.Instance);

        var result = await service.Create(new CreateUserDto
        {
            Name = "Budi",
            Email = "budi@example.com"
        });

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Budi", result.Name);
        Assert.Equal("budi@example.com", result.Email);
        Assert.Single(repository.Users);
    }

    [Fact]
    public async Task Update_ReturnsFalseWhenUserDoesNotExist()
    {
        var repository = new FakeUserRepository();
        var service = new UserService(repository, NullLogger<UserService>.Instance);

        var result = await service.Update(Guid.NewGuid(), new UpdateUserDto
        {
            Name = "Missing",
            Email = "missing@example.com"
        });

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_RemovesExistingUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Budi",
            Email = "budi@example.com"
        };
        var repository = new FakeUserRepository([user]);
        var service = new UserService(repository, NullLogger<UserService>.Instance);

        var result = await service.Delete(user.Id);

        Assert.True(result);
        Assert.Empty(repository.Users);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; }

        public FakeUserRepository(IEnumerable<User>? users = null)
        {
            Users = users?.ToList() ?? new List<User>();
        }

        public Task<List<User>> GetAll()
        {
            return Task.FromResult(Users);
        }

        public Task<PagedResult<User>> GetPaged(QueryParameters parameters)
        {
            return Task.FromResult(new PagedResult<User>
            {
                Items = Users,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = Users.Count,
                TotalPages = 1
            });
        }

        public Task<User?> GetById(Guid id)
        {
            return Task.FromResult(Users.FirstOrDefault(user => user.Id == id));
        }

        public Task<User> Create(User user)
        {
            Users.Add(user);
            return Task.FromResult(user);
        }

        public Task Update(User user)
        {
            var index = Users.FindIndex(item => item.Id == user.Id);
            if (index >= 0) Users[index] = user;

            return Task.CompletedTask;
        }

        public Task Delete(User user)
        {
            Users.Remove(user);
            return Task.CompletedTask;
        }
    }
}
