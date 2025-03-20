using System;
using DeepL.Model;

namespace VocabularyCards.Business.LLM;
public class OpenRouterPhraseExampleProviderFactory : IPhraseExampleProviderFactory
{
    public const string DefaultModelName = "deepseek/deepseek-chat:free";

    private readonly OpenRouterSettings _settings;
    private readonly Domain.Language _language;

    public OpenRouterPhraseExampleProviderFactory(OpenRouterSettings settings, Domain.Language language)
    {
        _settings = settings;
        _language = language;
    }

    public IPhraseExampleProvider Create()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("OpenAI Api Key is missed. Check Settings.");
        }

        return new OpenRouterPhraseExampleProvider(_settings.ApiKey, _settings.Model, _language);
    }
    public string[] GetModelsNames() => [
        DefaultModelName,
        "deepseek/deepseek-r1-zero:free",
        "deepseek/deepseek-r1:free",
        "meta-llama/llama-3.2-1b-instruct:free",
        "google/gemma-2-9b-it:free",
        "mistralai/mistral-7b-instruct:free",
        "nvidia/llama-3.1-nemotron-70b-instruct:free"
    ];
}
