namespace VocabularyCards.Domain;

public class PhraseExample
{
    public int Id { get; private set; }
    public int PhraseId { get; private set; }
    public string Text { get; set; }

    public PhraseExample(int id, int phraseId, string text)
    {
        Id = id;
        PhraseId = phraseId;
        Text = text;
    }
}
