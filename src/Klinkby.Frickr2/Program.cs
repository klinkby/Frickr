using Klinkby.Frickr2;
using Microsoft.Extensions.Logging;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger<ExifUpdateManager> logger = loggerFactory.CreateLogger<ExifUpdateManager>();

ExifUpdateManager exifUpdateManager = new ExifUpdateManager(
    args.Length > 0 ? args[0] : Environment.CurrentDirectory,
    "albums.json",
    "photo_*.json",
    loggerFactory.CreateLogger<ExifUpdateManager>()
);

CancellationTokenSource stopper = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    stopper.Cancel();
    eventArgs.Cancel = true;
    logger.LogWarning("Break signalled");
};

try
{
    await exifUpdateManager.Run(stopper.Token);
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Unhandled");
    return 1;
}