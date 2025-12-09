using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure;
using BudgetBuddy.App.Commands;

var repo = new InMemoryTransactionRepository();
var logger = new ConsoleLogger();
var importer = new CsvImporter(repo, logger);
var stats = new StatsService(repo);
var exporter = new ExportService(repo);

var router = new CommandRouter(repo, importer, stats, exporter);

Console.WriteLine("BudgetBuddy Console — type 'help' for commands.");

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true; // prevent application exit
    Console.WriteLine();
    Console.WriteLine("[INFO] Cancellation requested. Attempting to stop operation...");
    cts.Cancel();
};

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    // if previous cancellation happened, reset CTS
    if (cts.IsCancellationRequested)
        cts = new CancellationTokenSource();

    await router.RouteAsync(input!, cts.Token);
}
