using BudgetBuddy.Infrastructure;
using BudgetBuddy.Domain;
using Xunit;

namespace BudgetBuddy.Tests;

public class StatsServiceTests
{
    [Fact]
    public void MonthlyStats_ComputesCorrectValues()
    {
        var repo = new InMemoryTransactionRepository();
        repo.Add(new Transaction { Id = 1, Timestamp = new DateTime(2025, 1, 1), Amount = 100, Category = "Income" });
        repo.Add(new Transaction { Id = 2, Timestamp = new DateTime(2025, 1, 2), Amount = -50, Category = "Food" });

        var statsService = new StatsService(repo);

        var result = statsService.Monthly("2025-01");

        Assert.Equal(100, result.Income);
        Assert.Equal(-50, result.Expense);
        Assert.Equal(50, result.Net);
        Assert.Equal(75, result.Average); // average abs: (100 + |-50|) / 2
        Assert.Single(result.TopCategories);
        Assert.Equal("Food", result.TopCategories[0].Category);
        Assert.Equal(50, result.TopCategories[0].Total);
    }


    [Fact]
    public void YearlyStats_GroupsByMonth()
    {
        var repo = new InMemoryTransactionRepository();
        repo.Add(new Transaction { Id = 1, Timestamp = new DateTime(2025, 1, 1), Amount = 100 });
        repo.Add(new Transaction { Id = 2, Timestamp = new DateTime(2025, 2, 1), Amount = 200 });

        var stats = new StatsService(repo);
        var result = stats.Yearly(2025).ToList();

        Assert.Equal(2, result.Count);
    }
}
