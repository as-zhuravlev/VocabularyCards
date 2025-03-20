using System.Threading.Tasks;

namespace VocabularyCards.Business;
public interface IPhraseExampleProvider
{
    Task<string> GetExampleAsync(string phrase);
}
