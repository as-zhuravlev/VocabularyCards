namespace VocabularyCards.Business.Impl;

public class PhraseCardSettings
{
    public int MinTimeBeforePhraseRedisplayInMinutes { get; set; } = 10;
    public int PenaltyByViewsCount { get; set; } = 1001;
    public int PenaltyByTranslationViewsCount { get; set; } = -1000;
}
