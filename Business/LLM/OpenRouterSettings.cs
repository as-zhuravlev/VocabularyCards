namespace VocabularyCards.Business.LLM;
public class OpenRouterSettings
{
    public string? ApiKey { get; set; }

    public string Model { get; set; } = OpenRouterPhraseExampleProviderFactory.DefaultModelName;
}
