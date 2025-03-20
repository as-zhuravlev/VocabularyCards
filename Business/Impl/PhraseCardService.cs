using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VocabularyCards.Domain;
using VocabularyCards.Infra.DataAccess;

namespace VocabularyCards.Business.Impl;

internal class PhraseCardService : IPhraseCardService
{
    private readonly VcDataContext _context;
    private readonly PhraseCardSettings _settings;

    public PhraseCardService(PhraseCardSettings settings, VcDataContext context)
    {
        _context = context;
        _settings = settings;
    }

    public async Task<PhraseCard> GetNextAsync(Language from, Language to, int? collectionId = null)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        IQueryable<Phrase> query = GetSearchQuery(from, collectionId);

        long? minValue = query.Min(p =>
        (long?)p.ViewsСount * _settings.PenaltyByViewsCount +
        (long?)p.TranslationViewsСount * _settings.PenaltyByTranslationViewsCount
        + ((timestamp - p.LatestShowUnixTimestamp > _settings.MinTimeBeforePhraseRedisplayInMinutes * 60) ? 0 : 10_000_000));

        if (minValue == null)
        {
            return PhraseCard.CreateNew(from);
        }

        Phrase[] phrases = await GetSearchQuery(from, collectionId)
            .Where(p =>
        (long?)p.ViewsСount * _settings.PenaltyByViewsCount +
        (long?)p.TranslationViewsСount * _settings.PenaltyByTranslationViewsCount
        + ((timestamp - p.LatestShowUnixTimestamp > _settings.MinTimeBeforePhraseRedisplayInMinutes * 60) ? 0 : 10_000_000) == minValue)
            .ToArrayAsync();

        Random random = new((int)(DateTime.Now - DateTime.Today).TotalSeconds);
        int index = random.Next(phrases.Length);
        Phrase phrase = phrases[index];

        return new PhraseCard(phrase,
            await _context.Set<Translation>().Where(t => t.PhraseId == phrase.Id && t.Language == to).ToArrayAsync(),
            await _context.Set<PhraseExample>().Where(e => e.PhraseId == phrase.Id).ToArrayAsync());

    }

    private IQueryable<Phrase> GetSearchQuery(Language from, int? collectionId)
    {
        var q = _context.Set<Phrase>().Where(p => p.Language == from && p.Ignored == false);
        if (collectionId != null)
        {
            q = from p in q
                join cTp in _context.Set<CollectionToPhrase>().Where(c => c.CollectionId == collectionId)
                on p.Id equals cTp.PhaseId
                select p;
        }

        return q;
    }

    public async Task<SaveResult> SaveAsync(PhraseCard origin, PhraseCard current)
    {
        Phrase phrase;
        if (current.IsNew)
        {
            phrase = await FindExistedAsync(current.Phrase)
                 ?? (await InsertAsync(current.Phrase)).Single();

            foreach (var t in current.Translations)
            {
                SetPrivatePropertyValue(t, nameof(t.PhraseId), phrase.Id);
            }

            foreach (var e in current.Examples)
            {
                SetPrivatePropertyValue(e, nameof(e.PhraseId), phrase.Id);
            }
        }
        else
        {
            phrase = origin.Phrase;
        }

        if (!origin.IsNew && origin.Phrase.Text != current.Phrase.Text)
        {
            if (await _context.Set<Phrase>().AnyAsync(p => p.Text == current.Phrase.Text && p.Language == current.Phrase.Language))
            {
                return new SaveResult(SaveResultStatus.ConflictWithExisted, origin);

            }

            await _context.Set<Phrase>()
                .Where(p => p.Id == origin.Phrase.Id)
                .ExecuteUpdateAsync(p => p.SetProperty(p => p.Text, current.Phrase.Text));
        }

        return new SaveResult(SaveResultStatus.Ok,
            new PhraseCard(
            phrase,
            await PersistCollectionAsync(origin.Translations,
                                         current.Translations,
                                         t => t.Id,
                                         ids => (t) => ids.Contains(t.Id),
                                         t => t.Text,
                                         texts => (t) => texts.Contains(t.Text) && t.Language == phrase.Language),
            await PersistCollectionAsync(origin.Examples,
                                         current.Examples,
                                         t => t.Id,
                                         ids => (t) => ids.Contains(t.Id),
                                         t => t.Text,
                                         texts => (t) => texts.Contains(t.Text)))
            );
    }

    private async Task<Phrase?> FindExistedAsync(Phrase phrase)
    {
        return await _context.Set<Phrase>().Where(p =>
                    p.Language == phrase.Language
                    && p.Text == phrase.Text).SingleOrDefaultAsync();
    }

    private async Task<T[]> InsertAsync<T>(params T[] entity) where T : class
    {
        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        _context.Set<T>().AttachRange(entity);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return entity;
    }

    private async Task<T[]> PersistCollectionAsync<T, K, P>(
        T[] origin,
        T[] current,
        Expression<Func<T, K>> keySelector,
        Func<K[], Expression<Func<T, bool>>> keyInSetFilter,
        Expression<Func<T, P>> searchBy,
        Func<P[], Expression<Func<T, bool>>> searchInSetFilter) where T : class
    {
        Func<T, K> keySelectorFunc = keySelector.Compile();

        K[] keysToDelete = origin
            .ExceptBy(current.Select(keySelectorFunc), keySelectorFunc)
            .Select(keySelectorFunc)
            .ToArray();
        await _context.Set<T>().Where(keyInSetFilter(keysToDelete)).ExecuteDeleteAsync();

        T[] addingItems = current.Where(e => keySelectorFunc(e)?.Equals(default(K)) ?? false).ToArray();

        Func<T, P> searchByFunc = searchBy.Compile();
        P[] itemsToSearch = addingItems.Select(searchByFunc).ToArray();


        T[] existedItems = await _context.Set<T>().Where(searchInSetFilter(itemsToSearch)).ToArrayAsync();

        addingItems = addingItems.ExceptBy(existedItems.Select(keySelectorFunc), keySelectorFunc).ToArray();

        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        _context.Set<T>().AttachRange(addingItems);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return [.. existedItems, .. addingItems];
    }

    private static void SetPrivatePropertyValue<T>(object obj, string propName, T val)
    {
        Type t = obj.GetType();
        if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
            throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
        t.InvokeMember(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, obj, new object[] { val });
    }

    public async Task IncreaseViewsCountAsync(int pharaseId)
    {
        long timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _context.Set<Phrase>().Where(p => p.Id == pharaseId).ExecuteUpdateAsync(p =>
            p.SetProperty(p => p.ViewsСount, p => p.ViewsСount + 1)
            .SetProperty(p => p.LatestShowUnixTimestamp, timeStamp)
        );
    }

    public async Task IncreaseTranslationsViewCountAsync(int pharaseId)
    {
        await _context.Set<Phrase>().Where(p => p.Id == pharaseId).ExecuteUpdateAsync(p => p.SetProperty(p => p.TranslationViewsСount, p => p.TranslationViewsСount + 1));
    }

    public async Task ImportFileAsync(Language from, string path)
    {
        var fi = new FileInfo(path);

        if (!fi.Exists || fi.Length > 100_000)
        {
            return;
        }

        string[] lines = await File.ReadAllLinesAsync(path);

        foreach (string[] c in lines.Chunk(20))
        {

            string[] chunk =
                    c.Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim().ToLower())
                    .Distinct()
                    .ToArray();

            string[] existed = await _context
                .Set<Phrase>()
                .Where(p => p.Language == from && chunk.Contains(p.Text))
                .Select(p => p.Text)
                .ToArrayAsync();

            await InsertAsync(chunk.Except(existed).Select(t => new Phrase(default, from, t)).ToArray());
        };
    }

    public async Task DeleteAsync(int pharaseId)
    {
        await _context.Set<Phrase>().Where(p => p.Id == pharaseId).ExecuteDeleteAsync();
    }
}
