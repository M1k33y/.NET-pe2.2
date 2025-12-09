using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure
{
    public class InMemoryTransactionRepository :
    IRepository<Transaction, int>
    {
        private readonly Dictionary<int, Transaction> _store = new();

        public bool Add(Transaction t)
        {
            if (_store.ContainsKey(t.Id))
                return false;
            _store[t.Id] = t;
            return true;
        }

        public bool Remove(int id)
            => _store.Remove(id);

        public bool TryGet(int id, out Transaction? value)
            => _store.TryGetValue(id, out value);

        public IReadOnlyCollection<Transaction> All => _store.Values.ToList();
    }
}
