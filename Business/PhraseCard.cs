using VocabularyCards.Domain;

namespace VocabularyCards.Business;
public record PhraseCard(
    Phrase Phrase,
    Translation[] Translations,
    PhraseExample[] Examples
)
{
    public static PhraseCard CreateNew(Language language) =>
        new PhraseCard(new Phrase(default, language, string.Empty), [], []);

    public bool IsNew => Phrase.Id == 0;
}
