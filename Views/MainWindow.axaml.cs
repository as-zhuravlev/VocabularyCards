using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using VocabularyCards.ViewModels;
using System;

namespace VocabularyCards.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ImportButton_Click(object? sender, RoutedEventArgs e)
    {
        if ((await MessageBoxManager.GetMessageBoxStandard(
          "",
          "Imopt file should contain one word or phrase per line." + Environment.NewLine
          + "Do you want to continue?",
          icon: MsBox.Avalonia.Enums.Icon.Question,
          @enum: ButtonEnum.YesNo).ShowAsync()) == ButtonResult.No)
        {
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select file for import",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("Text Files") { Patterns = ["*.txt"] }]
        });

        if (files.Count > 0)
        {
            var vm = (MainWindowViewModel)DataContext!;
            await vm.ImportFileAsync(files[0].Path.LocalPath);
        }
    }

    private async void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext!;

        SettingsViewModel svm = new(vm.Container);
        SettingsWindow settingsWindow = new()
        {
            DataContext = svm
        };

        await settingsWindow.ShowDialog(this);
    }
}
