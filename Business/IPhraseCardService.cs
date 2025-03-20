using System.Threading.Tasks;
using VocabularyCards.Domain;

namespace VocabularyCards.Business;

public interface IPhraseCardService
{

    Task IncreaseViewsCountAsync(int pharaseId);
    Task DeleteAsync(int pharaseId);
    Task IncreaseTranslationsViewCountAsync(int pharaseId);

    Task<PhraseCard> GetNextAsync(Language from, Language to, int? collectionId = null);

    Task<SaveResult> SaveAsync(PhraseCard origin, PhraseCard current);

    Task ImportFileAsync(Language from, string path);
}
