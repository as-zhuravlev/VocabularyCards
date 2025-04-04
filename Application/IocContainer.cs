using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using VocabularyCards.Business;
using VocabularyCards.Business.Impl;
using VocabularyCards.Business.LLM;
using VocabularyCards.Business.Translators;
using VocabularyCards.Infra.DataAccess;

namespace VocabularyCards.Application;

public class IocContainer : IDisposable
{
    private readonly ILifetimeScope _container;

    private IocContainer(ILifetimeScope container)
    {
        _container = container;
    }

    public void ResolveScoped<T>(Action<T> action) where T : class
    {
        ILifetimeScope scope = _container.BeginLifetimeScope();
        action(scope.Resolve<T>());
        scope.Dispose();
    }

    public R ResolveScoped<T, R>(Func<T, R> action) where T : class
    {
        ILifetimeScope scope = _container.BeginLifetimeScope();
        R result = action(scope.Resolve<T>());
        scope.Dispose();
        return result;
    }

    public async Task ResolveScopedAsync<T>(Func<T, Task> action) where T : class
    {
        ILifetimeScope scope = _container.BeginLifetimeScope();
        await action(scope.Resolve<T>());
        await scope.DisposeAsync();
    }

    public async Task<R> ResolveScopedAsync<T, R>(Func<T, Task<R>> func) where T : class
    {
        ILifetimeScope scope = _container.BeginLifetimeScope();
        var result = await func(scope.Resolve<T>());
        await scope.DisposeAsync();
        return result;
    }

    public static IocContainer Create()
    {
        ContainerBuilder builder = new();

        builder.RegisterInstance(AppSettings.Load()).AsSelf().SingleInstance();
        builder.Register(ctx => ctx.Resolve<AppSettings>().ShowCardSettings).AsSelf().SingleInstance();

        string appDir = Path.GetDirectoryName(Environment.ProcessPath)!;
        string sqlFile = Path.Combine(appDir, "vc.sqlite");

        builder.Register(
        ctx => new DbContextOptionsBuilder<VcDataContext>()
            .UseSqlite($"Data Source={sqlFile};")
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options)
            .As<DbContextOptions<VcDataContext>>()
            .InstancePerLifetimeScope();

        builder.RegisterType<VcDataContext>().AsSelf().InstancePerLifetimeScope();

        builder.RegisterType<PhraseCardService>()
            .As<IPhraseCardService>()
            .InstancePerLifetimeScope();

        builder.Register(ctx => new DeepLTranslatorFactory(ctx.Resolve<AppSettings>().DeepLApiKey))
            .As<ITranslatorFactory>()
            .InstancePerLifetimeScope();

        builder.Register(ctx =>
        {
            var settings = ctx.Resolve<AppSettings>();
            return new OpenRouterPhraseExampleProviderFactory(
            settings.OpenRouterSettings,
            settings.TranslateFrom);
        })
           .As<IPhraseExampleProviderFactory>()
           .InstancePerLifetimeScope();

        return new IocContainer(builder.Build());
    }

    public void Dispose()
    {
        _container.Dispose();
    }
}
