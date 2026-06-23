using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.DTOs;
using MyApi.Extensions;
using MyApi.Models;

namespace MyApi.Tests;

public class QueryableExtensionsTests
{
    [Fact]
    public async Task ToPagedResultAsync_ReturnsRequestedPageWithMetadata()
    {
        await using var context = CreateContext();
        await SeedUsers(context);

        var result = await context.Users
            .OrderBy(user => user.Name)
            .ToPagedResultAsync(new QueryParameters
            {
                PageNumber = 2,
                PageSize = 2
            });

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
        Assert.Equal(["Charlie", "Dewi"], result.Items.Select(user => user.Name).ToArray());
    }

    [Fact]
    public async Task ApplySearch_SearchesAllStringProperties()
    {
        await using var context = CreateContext();
        await SeedUsers(context);

        var result = await context.Users
            .ApplySearch("example.net")
            .OrderBy(user => user.Name)
            .ToListAsync();

        Assert.Equal(["Charlie", "Dewi"], result.Select(user => user.Name).ToArray());
    }

    [Fact]
    public async Task ApplyFilters_FiltersByNamedProperty()
    {
        await using var context = CreateContext();
        await SeedUsers(context);

        var result = await context.Users
            .ApplyFilters(new Dictionary<string, string>
            {
                ["Email"] = "gmail"
            })
            .OrderBy(user => user.Name)
            .ToListAsync();

        Assert.Equal(["Budi", "Eka"], result.Select(user => user.Name).ToArray());
    }

    [Fact]
    public async Task ApplySorting_SortsDescendingByRequestedProperty()
    {
        await using var context = CreateContext();
        await SeedUsers(context);

        var result = await context.Users
            .ApplySorting("Name", "desc")
            .ToListAsync();

        Assert.Equal(["Eka", "Dewi", "Charlie", "Budi", "Andi"], result.Select(user => user.Name).ToArray());
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
            new User { Id = Guid.NewGuid(), Name = "Charlie", Email = "charlie@example.net" },
            new User { Id = Guid.NewGuid(), Name = "Dewi", Email = "dewi@example.net" },
            new User { Id = Guid.NewGuid(), Name = "Eka", Email = "eka@gmail.com" });

        await context.SaveChangesAsync();
    }
}
