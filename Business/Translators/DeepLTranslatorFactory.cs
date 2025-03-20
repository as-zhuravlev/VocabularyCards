using System;

namespace VocabularyCards.Business.Translators;

public class DeepLTranslatorFactory : ITranslatorFactory
{
    private readonly string? _apiKey;

    public DeepLTranslatorFactory(string? apiKey)
    {
        _apiKey = apiKey;
    }

    public ITranslator Create()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("DeepL Api Key is missed. Check Settings.");
        }

        return new DeepLTranslator(_apiKey);
    }
}
