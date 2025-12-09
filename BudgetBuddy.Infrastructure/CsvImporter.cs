using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure
{
    public class CsvImporter
    {
        private readonly IRepository<Transaction, int> _repo;

        private readonly ILogger _logger;
        public CsvImporter(IRepository<Transaction, int> repo, ILogger logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<(int imported, int duplicates, int malformed)> ImportFilesAsync(
    IEnumerable<string> paths,
    CancellationToken ct)
        {
            int imported = 0, duplicates = 0, malformed = 0;

            try
            {
                await Parallel.ForEachAsync(paths, ct, async (path, token) =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    string[] lines;

                    try
                    {
                        lines = await File.ReadAllLinesAsync(path, token);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Info($"Import cancelled while reading file '{path}'.");
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to read file '{path}': {ex.Message}");
                        return;
                    }

                    int lineNumber = 1;

                    foreach (var line in lines.Skip(1))
                    {
                        if (token.IsCancellationRequested)
                            return;

                        lineNumber++;

                        var result = TransactionFactory.TryCreate(line);
                        if (!result.IsSuccess)
                        {
                            Interlocked.Increment(ref malformed);
                            _logger.Warn($"Malformed row at line {lineNumber}: {result.Error}");
                            continue;
                        }

                        var transaction = result.Value!;

                        if (!_repo.Add(transaction))
                        {
                            Interlocked.Increment(ref duplicates);
                            _logger.Warn($"Duplicate ID skipped: {transaction.Id}");
                            continue;
                        }

                        Interlocked.Increment(ref imported);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                _logger.Warn("Import cancelled by user.");
            }
            catch (AggregateException ag)
            {
                // swallow parallel exceptions caused by cancellation
                if (ag.InnerExceptions.All(e => e is OperationCanceledException))
                {
                    _logger.Warn("Import cancelled during parallel execution.");
                }
                else
                {
                    _logger.Error("Unexpected error: " + ag);
                }
            }

            _logger.Info($"Import summary → Imported: {imported}, Duplicates: {duplicates}, Malformed: {malformed}");

            return (imported, duplicates, malformed);
        }

    }
}
