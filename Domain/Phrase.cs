namespace VocabularyCards.Domain;
public class Phrase
{
    public Phrase(int id, Language language, string text, bool ignored = false)
    {
        Id = id;
        Language = language;
        Text = text;
        Ignored = ignored;
    }

    public int Id { get; private set; }
    public Language Language { get; private set; }
    public string Text { get; private set; }
    public int ViewsСount { get; private set; }
    public int TranslationViewsСount { get; private set; }
    public bool Ignored { get; private set; }
    public long LatestShowUnixTimestamp { get; private set; }
}



