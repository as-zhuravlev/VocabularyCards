using VocabularyCards.Application;
using VocabularyCards.Business;

namespace VocabularyCards.ViewModels;

public class SettingsViewModel
{

    private readonly IocContainer _container;

    public SettingsViewModel(IocContainer container)
    {
        _container = container;
        var settings = _container.ResolveScoped((AppSettings s) => s);

        DeepLApiKey = settings.DeepLApiKey;
        OpenRouterApiKey = settings.OpenRouterSettings.ApiKey;
        OpenRouterModel = settings.OpenRouterSettings.Model;
        OpenRouterModels = _container.ResolveScoped((IPhraseExampleProviderFactory f) => f.GetModelsNames());
        MinTimeBeforePhraseRedisplayInMinutes = settings.ShowCardSettings.MinTimeBeforePhraseRedisplayInMinutes;
        PenaltyByViewsCount = settings.ShowCardSettings.PenaltyByViewsCount;
        PenaltyByTranslationViewsCount = settings.ShowCardSettings.PenaltyByTranslationViewsCount;
    }

    public string? DeepLApiKey { get; set; }

    public string? OpenRouterApiKey { get; set; }

    public string OpenRouterModel { get; set; }

    public string[] OpenRouterModels { get; }

    public int MinTimeBeforePhraseRedisplayInMinutes { get; set; } = 10;
    public int PenaltyByViewsCount { get; set; } = 1001;
    public int PenaltyByTranslationViewsCount { get; set; } = -1000;

    public void Save()
    {
        var settings = _container.ResolveScoped((AppSettings s) => s);

        settings.DeepLApiKey = DeepLApiKey?.Trim();
        settings.OpenRouterSettings.ApiKey = OpenRouterApiKey;
        settings.OpenRouterSettings.Model = OpenRouterModel;
        settings.ShowCardSettings.MinTimeBeforePhraseRedisplayInMinutes = MinTimeBeforePhraseRedisplayInMinutes;
        settings.ShowCardSettings.PenaltyByViewsCount = PenaltyByViewsCount;
        settings.ShowCardSettings.PenaltyByTranslationViewsCount = PenaltyByTranslationViewsCount;

        settings.Save();
    }

}
