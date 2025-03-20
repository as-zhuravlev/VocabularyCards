using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using VocabularyCards.Application;
using VocabularyCards.Infra.DataAccess;
using VocabularyCards.ViewModels;
using VocabularyCards.Views;

namespace VocabularyCards;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var conatainer = IocContainer.Create();

            InitDb(conatainer);

            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(conatainer),
            };

            desktop.Exit += (s, e) =>
            {
                conatainer.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitDb(IocContainer container)
    {
        container.ResolveScoped((VcDataContext ctx) =>
        {
            ctx.Database.EnsureCreated();
        });
    }

}
