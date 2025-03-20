using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using VocabularyCards.Application;
using VocabularyCards.Business;
using VocabularyCards.Domain;

namespace VocabularyCards.ViewModels;

public partial class PhraseCardViewModel : ViewModelBase
{
    private readonly IocContainer _iocContainer;

    public PhraseCard PhraseCard { get; private set; }
    public Language From { get; }
    private readonly Language _to;

    [ObservableProperty]
    private bool _isEditable;

    [ObservableProperty]
    private bool _isExampleLoading;

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private ObservableCollection<Translation> _translations = [];

    [ObservableProperty]
    private ObservableCollection<PhraseExample> _examples = [];

    public bool IsNew => PhraseCard.IsNew;


    public PhraseCardViewModel(
        IocContainer iocContainer,
        PhraseCard phraseCard,
        Language from,
        Language to)
    {
        _iocContainer = iocContainer;
        PhraseCard = phraseCard;
        From = from;
        _to = to;

        _isEditable = phraseCard.IsNew;

        _text = phraseCard.Phrase.Text;

        foreach (Translation translation in phraseCard.Translations)
        {
            _translations.Add(translation);
        }

        foreach (PhraseExample example in phraseCard.Examples)
        {
            _examples.Add(example);
        }
    }

    public event EventHandler<EventArgs>? CardDeleted;

    protected virtual void OnCardDeleted(EventArgs e)
    {
        CardDeleted?.Invoke(this, e);
    }

    public void EnableEdit(string _)
    {
        IsEditable = true;
    }

    public async void Save(string _)
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        PhraseCard current = new(
            new Phrase(
                PhraseCard.Phrase.Id,
                From,
                Text,
                PhraseCard.Phrase.Ignored),
            [.. Translations],
            [.. Examples]
            );

        var result = await _iocContainer.ResolveScopedAsync((IPhraseCardService service) =>
        {
            return service.SaveAsync(PhraseCard, current);
        });


        if (result.Status == SaveResultStatus.Ok)
        {
            IsEditable = false;
            PhraseCard = result.Card;
        }
        else
        {
            OnMessageRaised(new MessageEventArgs(MessageSeverity.Error, "This phrase has already been in your vocablurary"));
        }


    }

    public async Task IncreaseTranslationViewsCountAsync()
    {
        await _iocContainer.ResolveScopedAsync(async (IPhraseCardService service) =>
        {
            await service.IncreaseTranslationsViewCountAsync(PhraseCard.Phrase.Id);
        });
    }

    public void AddTranslation(string translation)
    {
        translation = translation.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(translation) || Translations.Any(t => t.Text == translation))
        {
            return;
        }

        Translations.Add(new Translation(
                default,
                PhraseCard.Phrase.Id,
                translation,
                _to,
                TranslationType.Manual
            ));
    }

    public void AddExample(string example)
    {
        example = example.Trim();

        if (string.IsNullOrWhiteSpace(example) || Examples.Any(t => t.Text == example))
        {
            return;
        }

        Examples.Add(new PhraseExample(
                default,
                PhraseCard.Phrase.Id,
                example
            ));
    }

    public async void TranslateAsync(string _)
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        Text = Text.Trim().ToLower();

        TranslationType type;
        string translation = string.Empty;
        try
        {

            await _iocContainer.ResolveScopedAsync(async (ITranslatorFactory translatorFactory) =>
            {
                var translator = translatorFactory.Create();
                type = translator.Type;
                translation = await translator.TranslateAsync(Text, From, _to);
            });
        }
        catch (InvalidOperationException ex)
        {
            OnMessageRaised(new MessageEventArgs(MessageSeverity.Error, ex.Message));
            return;
        }

        translation = translation.ToLower();

        if (Translations.Any(t => t.Text == translation))
        {
            OnMessageRaised(new MessageEventArgs(MessageSeverity.Info, "Phrase has already been translated"));
            return;
        }

        Translations.Add(new Translation(
                default,
                PhraseCard.Phrase.Id,
                translation.ToLower(),
                _to,
                TranslationType.DeepL
            ));
    }

    public async void GetExampleAsync(string _)
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        Text = Text.Trim().ToLower();

        string example = string.Empty;
        IsExampleLoading = true;
        try
        {

            await _iocContainer.ResolveScopedAsync(async (IPhraseExampleProviderFactory factory) =>
            {
                IPhraseExampleProvider provider = factory.Create();
                example = await provider.GetExampleAsync(Text);
            });
        }
        catch (InvalidOperationException ex)
        {
            OnMessageRaised(new MessageEventArgs(MessageSeverity.Error, ex.Message));
            return;
        }
        finally
        {
            IsExampleLoading = false;
        }

        if (Examples.Any(t => t.Text == example))
        {
            OnMessageRaised(new MessageEventArgs(MessageSeverity.Info, "Phrase has already has this example"));
            return;
        }

        Examples.Add(new PhraseExample(
                default,
                PhraseCard.Phrase.Id,
                example));
    }

    public async Task DeleteAsync()
    {
        await _iocContainer.ResolveScopedAsync(async (IPhraseCardService service) =>
        {
            await service.DeleteAsync(PhraseCard.Phrase.Id);

        });
        OnCardDeleted(new EventArgs());
    }
}
