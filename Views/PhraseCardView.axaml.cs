using System;
using System.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using VocabularyCards.ViewModels;

namespace VocabularyCards.Views;

public partial class PhraseCardView : UserControl
{
    public PhraseCardView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is PhraseCardViewModel vm)
            {
                PhraseGrid.IsVisible = !vm.PhraseCard.IsNew;


                vm.MessageRaised += async (s, e) =>
                {
                    (Icon icon, string title) = e.Severity switch
                    {
                        MessageSeverity.Info => (Icon.None, "Info"),
                        MessageSeverity.Warning => (Icon.Warning, "Warning"),
                        MessageSeverity.Error => (Icon.Error, "Error"),
                        _ => throw new NotImplementedException()
                    };

                    await MessageBoxManager.GetMessageBoxStandard(title, e.Message, icon: icon).ShowAsync();
                }; ;
            }
        };
    }

    private async void PhraseGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PhraseGrid.IsVisible = false;
        if (DataContext is PhraseCardViewModel vm)
        {
            await vm.IncreaseTranslationViewsCountAsync();
        }
    }

    private void DeleteItem_Click(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;
        var itemsControl = button.FindLogicalAncestorOfType<ItemsControl>()!;
        var items = (IList)itemsControl.ItemsSource!;
        items.Remove(button.DataContext);
    }

    private void addTranslationButton_Click(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => newTranslation.Text = string.Empty, DispatcherPriority.Background);
    }

    private void newTranslationTextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter
            && DataContext is PhraseCardViewModel vm
            && !string.IsNullOrWhiteSpace(newTranslation.Text))
        {
            vm.AddTranslation(newTranslation.Text);
            newTranslation.Text = string.Empty;
        }
    }

    private void addExampleButton_Click(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => newExampleTextBox.Text = string.Empty, DispatcherPriority.Background);
    }

    private void newExampleTextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter
         && DataContext is PhraseCardViewModel vm
         && !string.IsNullOrWhiteSpace(newExampleTextBox.Text))
        {
            vm.AddExample(newExampleTextBox.Text);
            newExampleTextBox.Text = string.Empty;
        }
    }

    private async void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        if ((await MessageBoxManager.GetMessageBoxStandard(
            "",
            "Do you really want to delete this card?",
            icon: MsBox.Avalonia.Enums.Icon.Question,
            @enum: ButtonEnum.YesNo).ShowAsync()) == ButtonResult.Yes
            && DataContext is PhraseCardViewModel vm)
        {
            await vm.DeleteAsync();
        }

    }
}
