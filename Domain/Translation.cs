namespace VocabularyCards.Domain;

public record class Translation(
    int Id,
    int PhraseId,
    string Text,
    Language Language,
    TranslationType Type);

