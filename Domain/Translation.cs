namespace VocabularyCards.Domain;

public class Translation
{
    public Translation(int id,
        int phraseId,
        string text,
        Language language,
        TranslationType type)
    {
        Id = id;
        PhraseId = phraseId;
        Text = text;
        Language = language;
        Type = type;
    }

    public int Id { get; private set; }
    public int PhraseId { get; private set; }
    public string Text { get; private set; }
    public Language Language { get; private set; }
    public TranslationType Type { get; private set; }
}
