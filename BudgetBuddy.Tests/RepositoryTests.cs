using BudgetBuddy.Infrastructure;
using BudgetBuddy.Domain;
using Xunit;

namespace BudgetBuddy.Tests;

public class RepositoryTests
{
    [Fact]
    public void Add_NewItem_ReturnsTrue()
    {
        var repo = new InMemoryTransactionRepository();
        var tx = new Transaction { Id = 1 };

        var result = repo.Add(tx);

        Assert.True(result);
    }

    [Fact]
    public void Add_Duplicate_ReturnsFalse()
    {
        var repo = new InMemoryTransactionRepository();
        var tx = new Transaction { Id = 1 };

        repo.Add(tx);
        var result = repo.Add(tx);

        Assert.False(result);
    }

    [Fact]
    public void Remove_Existing_ReturnsTrue()
    {
        var repo = new InMemoryTransactionRepository();
        var tx = new Transaction { Id = 1 };

        repo.Add(tx);
        var removed = repo.Remove(1);

        Assert.True(removed);
    }

    [Fact]
    public void TryGet_Existing_ReturnsTrue()
    {
        var repo = new InMemoryTransactionRepository();
        var tx = new Transaction { Id = 1 };

        repo.Add(tx);

        var result = repo.TryGet(1, out var fetched);

        Assert.True(result);
        Assert.NotNull(fetched);
        Assert.Equal(1, fetched!.Id);
    }
}
