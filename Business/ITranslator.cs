using System.Threading.Tasks;
using VocabularyCards.Domain;

namespace VocabularyCards.Business;

public interface ITranslator
{
    TranslationType Type { get; }

    public Task<string> TranslateAsync(string text, Language from, Language to);
}
