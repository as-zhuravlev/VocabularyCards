using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using VocabularyCards.Application;
using VocabularyCards.Business;
using VocabularyCards.Domain;

namespace VocabularyCards.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public IocContainer Container { get; }

    [ObservableProperty]
    private Language _languageFrom;

    [ObservableProperty]
    private Language _languageTo;

    [ObservableProperty]
    private Task<PhraseCollection[]> _phraseCollections;

    [ObservableProperty]
    private Task<PhraseCardViewModel> _currentPhraseCard;

    public MainWindowViewModel(IocContainer container)
    {
        Container = container;
        (_languageFrom, _languageTo)
            = container.ResolveScoped((AppSettings s)
            => (s.TranslateFrom, s.TranslateTo));
        _currentPhraseCard = GetNextCard();
        _phraseCollections = Task.FromResult(Array.Empty<PhraseCollection>());
    }

    private Task<PhraseCardViewModel> GetNextCard(bool showBlanck = false)
    {
        return Container.ResolveScopedAsync(async (IPhraseCardService service) =>
        {
            if (CurrentPhraseCard != null)
            {
                var current = await CurrentPhraseCard;
                await service.IncreaseViewsCountAsync(current.PhraseCard.Phrase.Id);
            }
            var card = new PhraseCardViewModel(
                Container,
                showBlanck ? PhraseCard.CreateNew(LanguageFrom) : await service.GetNextAsync(LanguageFrom, LanguageTo),
                LanguageFrom,
                LanguageTo);

            card.CardDeleted += (_, _) =>
            {
                CurrentPhraseCard = GetNextCard(false);
            };
            return card;
        });
    }

    public void ShowNextCard(string _)
    {
        CurrentPhraseCard = GetNextCard();
    }

    public void AddNewCard(string _)
    {
        CurrentPhraseCard = GetNextCard(true);
    }

    public Language[] Languages { get; } = Enum.GetValues<Language>();

    partial void OnLanguageFromChanged(Language value)
    {
        if (value == LanguageTo)
        {
            LanguageTo = GetNextLanguage(LanguageTo);
        }

        Container.ResolveScoped((AppSettings s) =>
        {
            s.TranslateFrom = LanguageFrom;
            s.Save();
        });

        CurrentPhraseCard = GetNextCard();
    }

    partial void OnLanguageToChanged(Language value)
    {
        if (value == LanguageFrom)
        {
            LanguageFrom = GetNextLanguage(LanguageFrom);
        }

        Container.ResolveScoped((AppSettings s) =>
        {
            s.TranslateTo = LanguageTo;
            s.Save();
        });

        CurrentPhraseCard = GetNextCard();
    }

    private Language GetNextLanguage(Language language) =>
        (Language)(((int)language + 1) % Languages.Length);

    public async Task ImportFileAsync(string path)
    {
        await Container.ResolveScopedAsync(
            async (IPhraseCardService s) => await s.ImportFileAsync(LanguageFrom, path));
    }
}
