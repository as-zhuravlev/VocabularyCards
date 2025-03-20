namespace VocabularyCards.Business;
internal interface IPhraseExampleProviderFactory
{
    IPhraseExampleProvider Create();

    string[] GetModelsNames();
}
