using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Avalonia.Data;
using Avalonia.Data.Converters;
using VocabularyCards.Domain;

namespace VocabularyCards.Views.Converters;
public class ChatGptUrlConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return targetType == typeof(Uri)
            && values.Count == 2
            && values[0] is string prompt
            && values[1] is Language language
            ? new Uri($"https://chatgpt.com/?q=" + HttpUtility.UrlEncode(
                GetPrompt(prompt, language)))
            : (object)new BindingNotification("Can not cast value to url");
    }

    private string GetPrompt(string phrase, Language language) => language switch
    {
        Domain.Language.English => $"explain and write a one short sentence with '{phrase}'",
        Domain.Language.Spanish => $"explicar y escribir una oración corta con '{phrase}'",
        Domain.Language.Portuguese => $"explicar e escrever uma frase curta com '{phrase}'",
        Domain.Language.French => $"expliquer et écrire une courte phrase avec '{phrase}'",
        Domain.Language.Russian => $"объясни и напиши короткое предложение с '{phrase}'",
        Domain.Language.German => $"erklären und schreiben einen kurzen Satz mit '{phrase}'",
        _ => throw new NotImplementedException()
    };
}
