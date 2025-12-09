using BudgetBuddy.Infrastructure;
using BudgetBuddy.Domain;
using Xunit;

namespace BudgetBuddy.Tests;

public class TransactionFactoryTests
{
    [Fact]
    public void TryCreate_ValidRow_ReturnsSuccess()
    {
        string csv = "1,2025-01-01,Store,-10.50,USD,Food";

        var result = TransactionFactory.TryCreate(csv);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal(new DateTime(2025, 1, 1), result.Value.Timestamp);
        Assert.Equal("Store", result.Value.Payee);
        Assert.Equal(-10.50m, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency);
        Assert.Equal("Food", result.Value.Category);
    }

    [Fact]
    public void TryCreate_InvalidId_ReturnsFail()
    {
        string csv = "abc,2025-01-01,Store,-10,USD,Food";

        var result = TransactionFactory.TryCreate(csv);

        Assert.False(result.IsSuccess);
        Assert.Contains("ID", result.Error!);
    }

    [Fact]
    public void TryCreate_AmountOutOfRange_ReturnsFail()
    {
        string csv = "1,2025-01-01,Store,2000000,USD,Food";

        var result = TransactionFactory.TryCreate(csv);

        Assert.False(result.IsSuccess);
        Assert.Contains("Amount", result.Error!);
    }

    [Fact]
    public void TryCreate_EmptyPayee_ReturnsFail()
    {
        string csv = "1,2025-01-01,,10,USD,Food";

        var result = TransactionFactory.TryCreate(csv);

        Assert.False(result.IsSuccess);
        Assert.Contains("Payee", result.Error!);
    }
}
