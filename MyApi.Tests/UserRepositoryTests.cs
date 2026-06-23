using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.DTOs;
using MyApi.Models;
using MyApi.Repositories;

namespace MyApi.Tests;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetPaged_AppliesSearchFilterSortAndPagination()
    {
        await using var context = CreateContext();
        await SeedUsers(context);
        var repository = new UserRepository(context);

        var result = await repository.GetPaged(new QueryParameters
        {
            PageNumber = 1,
            PageSize = 1,
            Search = "gmail",
            SortBy = "Name",
            SortDirection = "desc"
        });

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Eka", result.Items[0].Name);
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task SeedUsers(AppDbContext context)
    {
        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), Name = "Andi", Email = "andi@example.com" },
            new User { Id = Guid.NewGuid(), Name = "Budi", Email = "budi@gmail.com" },
            new User { Id = Guid.NewGuid(), Name = "Eka", Email = "eka@gmail.com" });

        await context.SaveChangesAsync();
    }
}
