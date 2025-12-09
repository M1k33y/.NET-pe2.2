using BudgetBuddy.Infrastructure;
using BudgetBuddy.Domain;

namespace BudgetBuddy.App.Commands;

public class CommandRouter
{
    private readonly IRepository<Transaction, int> _repo;
    private readonly CsvImporter _importer;
    private readonly StatsService _stats;
    private readonly ExportService _exporter;

    public CommandRouter(
        IRepository<Transaction, int> repo,
        CsvImporter importer,
        StatsService stats,
        ExportService exporter)
    {
        _repo = repo;
        _importer = importer;
        _stats = stats;
        _exporter = exporter;
    }

    public async Task RouteAsync(string input, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0];

        switch (cmd)
        {
            case "help":
                PrintHelp();
                break;

            case "exit":
                Environment.Exit(0);
                break;

            case "import":
                await HandleImport(parts.Length > 1 ? parts[1] : "", token);
                break;

            case "list":
                await HandleList(parts.Length > 1 ? parts[1] : "");
                break;

            case "stats":
                await HandleStats(parts.Length > 1 ? parts[1] : "");
                break;

            case "export":
                await HandleExport(parts.Length > 1 ? parts[1] : "", token);
                break;

            case "set":
            case "rename":
            case "remove":
                await HandleEditCommands(cmd, parts.Length > 1 ? parts[1] : "", token);
                break;

            default:
                Console.WriteLine("Unknown command. Type 'help' for help.");
                break;
        }
    }

    private Task HandleEditCommands(string cmd, string args, CancellationToken token)
    {
        var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // -----------------------------
        // 1) set category <id> <name>
        // -----------------------------
        if (cmd == "set" && parts.Length >= 3 && parts[0] == "category")
        {
            if (!int.TryParse(parts[1], out int id))
            {
                Console.WriteLine("Invalid ID.");
                return Task.CompletedTask;
            }

            string newCategory = string.Join(' ', parts.Skip(2));

            if (!_repo.TryGet(id, out var tx) || tx == null)
            {
                Console.WriteLine("404 Not Found.");
                return Task.CompletedTask;
            }

            tx.Category = newCategory;
            Console.WriteLine("200 OK");
            return Task.CompletedTask;
        }

        // -----------------------------
        // 2) rename category <old> <new>
        // -----------------------------
        if (cmd == "rename" && parts.Length >= 3 && parts[0] == "category")
        {
            string oldCat = parts[1].ToLowerInvariant();
            string newCat = string.Join(' ', parts.Skip(2));

            int count = 0;

            foreach (var t in _repo.All)
            {
                if (t.Category.ToLowerInvariant() == oldCat)
                {
                    t.Category = newCat;
                    count++;
                }
            }

            Console.WriteLine($"Updated {count} records.");
            return Task.CompletedTask;
        }

        // -----------------------------
        // 3) remove <id>
        // -----------------------------
        if (cmd == "remove" && parts.Length == 1)
        {
            if (!int.TryParse(parts[0], out int id))
            {
                Console.WriteLine("Invalid ID.");
                return Task.CompletedTask;
            }

            if (_repo.Remove(id))
                Console.WriteLine("200 OK");
            else
                Console.WriteLine("404 Not Found");

            return Task.CompletedTask;
        }

        Console.WriteLine("Invalid edit command.");
        return Task.CompletedTask;
    }

    private void PrintHelp()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("import <file1.csv> [file2.csv ...]");
        Console.WriteLine("list all");
        Console.WriteLine("list month <yyyy-MM>");
        Console.WriteLine("stats month <yyyy-MM>");
        Console.WriteLine("export json <path>");
        Console.WriteLine("export csv <path>");
        Console.WriteLine("exit");
        Console.WriteLine("by category <name>");
        Console.WriteLine("over <amount>");
        Console.WriteLine("search <text>");
        Console.WriteLine("stats yearly <yyyy>");

    }

    private async Task HandleImport(string args, CancellationToken token)
    {
        var files = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (files.Length == 0)
        {
            Console.WriteLine("Usage: import <file1.csv> [file2.csv]");
            return;
        }

        var (imported, duplicates, malformed) = await _importer.ImportFilesAsync(files, token);
        Console.WriteLine($"Imported: {imported}, Duplicates: {duplicates}, Malformed: {malformed}");
    }

    private Task HandleList(string args)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" list all");
            Console.WriteLine(" list month <yyyy-MM>");
            return Task.CompletedTask;
        }

        var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // list all
        if (parts.Length == 1 && parts[0] == "all")
        {
            if (!_repo.All.Any())
            {
                Console.WriteLine("No transactions found.");
                return Task.CompletedTask;
            }

            foreach (var t in _repo.All.OrderBy(t => t.Timestamp))
            {
                Console.WriteLine($"{t.Id} | {t.Timestamp:yyyy-MM-dd} | {t.Payee} | {t.Amount} | {t.Category}");
            }
            return Task.CompletedTask;
        }

        // list month 2025-01
        if (parts.Length == 2 && parts[0] == "month")
        {
            var month = parts[1];
            var tx = _repo.All
                .Where(t => t.Timestamp.MonthKey() == month)
                .OrderBy(t => t.Timestamp)
                .ToList();

            if (!tx.Any())
            {
                Console.WriteLine($"No transactions found for {month}.");
                return Task.CompletedTask;
            }

            foreach (var t in tx)
            {
                Console.WriteLine($"{t.Id} | {t.Timestamp:yyyy-MM-dd} | {t.Payee} | {t.Amount} | {t.Category}");
            }

            return Task.CompletedTask;
        }

        // list by category <name>
        if (parts.Length >= 3 && parts[0] == "by" && parts[1] == "category")
        {
            string category = string.Join(' ', parts.Skip(2));
            category = category.ToLowerInvariant();

            var tx = _repo.All
                .Where(t => t.Category.ToLowerInvariant().Contains(category))
                .OrderBy(t => t.Timestamp)
                .ToList();

            if (!tx.Any())
            {
                Console.WriteLine($"No transactions found for category containing '{category}'.");
                return Task.CompletedTask;
            }

            foreach (var t in tx)
                Console.WriteLine($"{t.Id} | {t.Timestamp:yyyy-MM-dd} | {t.Payee} | {t.Amount} | {t.Category}");

            return Task.CompletedTask;
        }

        // list over <amount>
        if (parts.Length == 2 && parts[0] == "over")
        {
            if (!decimal.TryParse(parts[1], out var threshold))
            {
                Console.WriteLine("Invalid amount.");
                return Task.CompletedTask;
            }

            var tx = _repo.All
                .Where(t => t.Amount >= threshold)
                .OrderBy(t => t.Timestamp)
                .ToList();

            if (!tx.Any())
            {
                Console.WriteLine($"No transactions found with Amount >= {threshold}.");
                return Task.CompletedTask;
            }

            foreach (var t in tx)
                Console.WriteLine($"{t.Id} | {t.Timestamp:yyyy-MM-dd} | {t.Payee} | {t.Amount} | {t.Category}");

            return Task.CompletedTask;
        }

        // list search <text>
        if (parts.Length >= 2 && parts[0] == "search")
        {
            string text = string.Join(' ', parts.Skip(1)).ToLowerInvariant();

            var tx = _repo.All
                .Where(t =>
                    t.Payee.ToLowerInvariant().Contains(text) ||
                    t.Category.ToLowerInvariant().Contains(text))
                .OrderBy(t => t.Timestamp)
                .ToList();

            if (!tx.Any())
            {
                Console.WriteLine($"No transactions found matching '{text}'.");
                return Task.CompletedTask;
            }

            foreach (var t in tx)
                Console.WriteLine($"{t.Id} | {t.Timestamp:yyyy-MM-dd} | {t.Payee} | {t.Amount} | {t.Category}");

            return Task.CompletedTask;
        }

        Console.WriteLine("Unknown list command. Try:");
        Console.WriteLine(" list all");
        Console.WriteLine(" list month <yyyy-MM>");
        return Task.CompletedTask;
    }

    private Task HandleStats(string args)
    {
        var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // stats yearly <yyyy>
        if (parts.Length == 2 && parts[0] == "yearly")
        {
            if (!int.TryParse(parts[1], out int year))
            {
                Console.WriteLine("Invalid year.");
                return Task.CompletedTask;
            }

            var result = _stats.Yearly(year).ToList();

            if (!result.Any())
            {
                Console.WriteLine($"No transactions found for year {year}.");
                return Task.CompletedTask;
            }

            Console.WriteLine("Month     | Income      | Expense     | Net");
            Console.WriteLine("-----------------------------------------------");

            foreach (var r in result)
            {
                // r este un obiect anonim, îl serializăm rapid în dictionary
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    System.Text.Json.JsonSerializer.Serialize(r));

                Console.WriteLine(
                    $"{dict["Month"],-10} | {dict["Income"],-11} | {dict["Expense"],-11} | {dict["Net"]}");
            }

            return Task.CompletedTask;
        }
        if (parts.Length == 2 && parts[0] == "month")
        {
            var month = parts[1];
            var result = _stats.Monthly(month);

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            return Task.CompletedTask;
        }

        Console.WriteLine("Usage: stats month <yyyy-MM>");
        return Task.CompletedTask;
    }

    private async Task HandleExport(string args, CancellationToken token)
    {
        var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            Console.WriteLine("Usage: export json <path> | export csv <path>");
            return;
        }

        string format = parts[0];
        string path = parts[1];

        // Step 1: check if file exists
        if (File.Exists(path))
        {
            Console.WriteLine($"File '{path}' already exists. Overwrite? (y/n)");

            var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (answer != "y")
            {
                Console.WriteLine("Export cancelled.");
                return;
            }
        }

        // Step 2: perform export
        try
        {
            switch (format)
            {
                case "json":
                    await _exporter.ExportJsonAsync(path, token);
                    Console.WriteLine("Exported JSON.");
                    break;

                case "csv":
                    await _exporter.ExportCsvAsync(path, token);
                    Console.WriteLine("Exported CSV.");
                    break;

                default:
                    Console.WriteLine("Unknown export type.");
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[INFO] Export cancelled by user.");
        }
    }
}
