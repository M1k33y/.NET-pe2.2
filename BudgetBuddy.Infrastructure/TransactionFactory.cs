using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure
{
    public static class TransactionFactory
    {
        // CSV: Id,Timestamp,Payee,Amount,Currency,Category
        public static Result<Transaction> TryCreate(string line)
        {
            var parts = line.Split(',');
            if (parts.Length < 5)
                return Result<Transaction>.Fail("Incorrect number of columns (expected at least 5)");

            // ----------------------
            // ID
            // ----------------------
            if (!int.TryParse(parts[0], out int id))
                return Result<Transaction>.Fail("Invalid ID format");

            if (id <= 0)
                return Result<Transaction>.Fail("ID must be greater than 0");

            // ----------------------
            // Date
            // ----------------------
            if (!parts[1].TryDate(out DateTime timestamp))
                return Result<Transaction>.Fail("Invalid date format (expected yyyy-MM-dd)");

            // ----------------------
            // Payee
            // ----------------------
            string payee = parts[2].Trim();
            if (string.IsNullOrWhiteSpace(payee))
                return Result<Transaction>.Fail("Payee cannot be empty");

            // ----------------------
            // Amount
            // ----------------------
            if (!parts[3].TryDec(out decimal amount))
                return Result<Transaction>.Fail("Invalid amount value");

            if (amount < -1_000_000m || amount > 1_000_000m)
                return Result<Transaction>.Fail("Amount out of allowed range (-1,000,000 to 1,000,000)");

            // ----------------------
            // Currency
            // ----------------------
            string currency = parts[4].Trim();
            if (string.IsNullOrWhiteSpace(currency))
                return Result<Transaction>.Fail("Currency cannot be empty");

            // ----------------------
            // Category (optional)
            // ----------------------
            string category = (parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]))
                ? parts[5].Trim()
                : "Uncategorized";

            // ----------------------
            // SUCCESS → build object
            // ----------------------
            return Result<Transaction>.Ok(new Transaction
            {
                Id = id,
                Timestamp = timestamp,
                Payee = payee,
                Amount = amount,
                Currency = currency,
                Category = category
            });
        }
    }
    }
