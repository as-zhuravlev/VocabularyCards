using System;
using System.IO;
using System.Text.Json;
using VocabularyCards.Business.Impl;
using VocabularyCards.Business.LLM;
using VocabularyCards.Domain;

namespace VocabularyCards.Application;

public partial class AppSettings
{
    private const string FileName = "VocabularyCards.settings.json";

    public Language TranslateFrom { get; set; }

    public Language TranslateTo { get; set; }

    public string? DeepLApiKey { get; set; }

    public OpenRouterSettings OpenRouterSettings { get; set; }

    public PhraseCardSettings ShowCardSettings { get; set; }
    public AppSettings()
    {
        TranslateFrom = Language.English;
        TranslateTo = Language.Spanish;
        ShowCardSettings = new PhraseCardSettings();
        OpenRouterSettings = new OpenRouterSettings();
    }

    public void Save()
    {
        File.WriteAllText(GetPath(),
            JsonSerializer.Serialize(this,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    public static AppSettings Load()
    {
        string path = GetPath();
        return (File.Exists(path)
            ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path))
            : null) ?? new AppSettings();

    }

    private static string GetPath() => Path.Combine(Path.GetDirectoryName(Environment.ProcessPath ?? string.Empty)!, FileName);

}
