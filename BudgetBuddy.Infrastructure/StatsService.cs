using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure
{
    public class StatsService
    {
        private readonly IRepository<Transaction, int> _repo;

        public StatsService(IRepository<Transaction, int> repo)
            => _repo = repo;

        public MonthlyStats Monthly(string month)
        {
            var tx = _repo.All
                .Where(t => t != null && t.Timestamp.MonthKey() == month)
                .ToList();

            var income = tx.Where(t => t.Amount > 0).Sum(t => t.Amount);
            var expense = tx.Where(t => t.Amount < 0).Sum(t => t.Amount);
            var net = income + expense;

            var topCats = tx
                .Where(t => t.Amount < 0)
                .GroupBy(t => t.Category)
                .Select(g => new CategoryStat
                {
                    Category = g.Key,
                    Total = g.Sum(t => Math.Abs(t.Amount))
                })
                .OrderByDescending(x => x.Total)
                .Take(3)
                .ToList();

            var avg = tx.Select(t => t.Amount).AverageAbs();

            return new MonthlyStats
            {
                Income = income,
                Expense = expense,
                Net = net,
                Average = avg,
                TopCategories = topCats
            };
        }


        public IEnumerable<object> Yearly(int year)
        {
            var tx = _repo.All
                .Where(t => t != null)
                .Where(t => t.Timestamp.Year == year)
                .ToList();

            var monthly = tx
                .GroupBy(t => t.Timestamp.MonthKey())
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var income = g.Where(t => t.Amount > 0).Sum(t => t.Amount);
                    var expense = g.Where(t => t.Amount < 0).Sum(t => t.Amount);
                    var net = income + expense;

                    return new
                    {
                        Month = g.Key,
                        Income = income,
                        Expense = expense,
                        Net = net
                    };
                });

            return monthly;
        }
    }
}
