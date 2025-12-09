using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure
{
    public interface IRepository<T, TKey>
    {
        bool Add(T item);
        bool Remove(TKey id);
        bool TryGet(TKey id, out T? value);
        IReadOnlyCollection<T> All { get; }
    }
}
