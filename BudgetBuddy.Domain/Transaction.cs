using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain;

public sealed class Transaction
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Payee { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Category { get; set; } = "Uncategorized";
}
