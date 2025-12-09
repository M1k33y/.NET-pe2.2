using BudgetBuddy.Domain;
using Xunit;

namespace BudgetBuddy.Tests;

public class ExtensionsTests
{
    [Fact]
    public void TryDec_ParsesValidDecimal()
    {
        bool ok = "123.45".TryDec(out var value);

        Assert.True(ok);
        Assert.Equal(123.45m, value);
    }

    [Fact]
    public void TryDate_ParsesValidDate()
    {
        bool ok = "2025-01-01".TryDate(out var value);

        Assert.True(ok);
        Assert.Equal(new DateTime(2025, 1, 1), value);
    }

    [Fact]
    public void MonthKey_ReturnsCorrectFormat()
    {
        var date = new DateTime(2025, 1, 15);
        Assert.Equal("2025-01", date.MonthKey());
    }
}
