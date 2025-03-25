using System;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using VocabularyCards.Domain;

namespace VocabularyCards.Business.Translators;

public class DeepLTranslator : ITranslator
{
    private readonly string _apiKey;

    public DeepLTranslator(string apiKey)
    {
        _apiKey = apiKey;
    }

    private string GetLanguageCode(Domain.Language language, bool useBritishEnglish =false) =>
        language switch
        {
            Domain.Language.English => useBritishEnglish ? LanguageCode.EnglishBritish : LanguageCode.English,
            Domain.Language.Spanish => LanguageCode.Spanish,
            Domain.Language.Portuguese => LanguageCode.Portuguese,
            Domain.Language.French => LanguageCode.French,
            Domain.Language.Russian => LanguageCode.Russian,
            Domain.Language.German => LanguageCode.German,
            _ => throw new NotImplementedException()
        };


    public TranslationType Type => TranslationType.DeepL;

    public async Task<string> TranslateAsync(string text, Domain.Language from, Domain.Language to)
    {
        using var translator = new Translator(_apiKey);

        TextResult translatedText = await translator.TranslateTextAsync(
              text,
              GetLanguageCode(from),
              GetLanguageCode(to, useBritishEnglish: true));

        return translatedText.Text;
    }
}
