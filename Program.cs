using System;
using Avalonia;
using NLog;

namespace VocabularyCards;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile(fileName: "VocabularyCards.log");
        });

        try
        {
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            LogManager.GetCurrentClassLogger().Error(ex);
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
