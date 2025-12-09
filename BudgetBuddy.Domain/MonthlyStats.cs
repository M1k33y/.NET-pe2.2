namespace BudgetBuddy.Domain;

public class MonthlyStats
{
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
    public decimal Average { get; set; }
    public List<CategoryStat> TopCategories { get; set; } = new();
}

public class CategoryStat
{
    public string Category { get; set; } = "";
    public decimal Total { get; set; }
}
