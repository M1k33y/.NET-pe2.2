using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure
{
    public class ExportService
    {
        private readonly IRepository<Transaction, int> _repo;

        public ExportService(IRepository<Transaction, int> repo)
            => _repo = repo;

        public async Task ExportJsonAsync(string path, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, _repo.All, cancellationToken: ct);

            ct.ThrowIfCancellationRequested();
        }

        public async Task ExportCsvAsync(string path, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var writer = new StreamWriter(path);

            await writer.WriteLineAsync("Id,Timestamp,Payee,Amount,Currency,Category");

            foreach (var t in _repo.All)
            {
                ct.ThrowIfCancellationRequested();

                var line =
                    $"{t.Id},{t.Timestamp:yyyy-MM-dd},{t.Payee},{t.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture)},{t.Currency},{t.Category}";

                await writer.WriteLineAsync(line);
            }
        }
    }
}
