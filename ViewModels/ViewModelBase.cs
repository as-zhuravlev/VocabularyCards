using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VocabularyCards.ViewModels;

public class ViewModelBase : ObservableObject
{
    public event EventHandler<MessageEventArgs>? MessageRaised;

    protected virtual void OnMessageRaised(MessageEventArgs e)
    {
        MessageRaised?.Invoke(this, e);
    }
}
