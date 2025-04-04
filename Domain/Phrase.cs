namespace VocabularyCards.Domain;

public record Phrase(
    int Id,
    Language Language,
    string Text,
    bool Ignored = false,
    int ViewsСount = 0,
    int TranslationViewsСount = 0,
    long LatestShowUnixTimestamp = 0);
