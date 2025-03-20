using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DeepL.Model;
using VocabularyCards.Domain;

namespace VocabularyCards.Business.LLM;
public class OpenRouterPhraseExampleProvider : IPhraseExampleProvider
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly Domain.Language _language;

    public OpenRouterPhraseExampleProvider(string apiKey, string model, Domain.Language language)
    {
        _apiKey = apiKey;
        _model = model;
        _language = language;
    }

    public async Task<string> GetExampleAsync(string phrase)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var requestBody = new
        {
            model = _model,
            messages = new[] {
                new
                {
                    role = "user",
                    content = GetPrompt(phrase)
                }
            }
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var responseMessage = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", requestContent);
        responseMessage.EnsureSuccessStatusCode();

        string responseBody = await responseMessage.Content.ReadAsStringAsync();
        Response response = JsonSerializer.Deserialize<Response>(responseBody)!;
        return response.choices[0].message.content.Replace("\"", string.Empty);
    }


    private string GetPrompt(string phrase) => _language switch
    {
        Domain.Language.English => $"Write a one short sentence with '{phrase}'",
        Domain.Language.Spanish => $"Escribe una oración corta con '{phrase}'",
        Domain.Language.Portuguese => $"Escreve uma frase curta com '{phrase}'",
        Domain.Language.French => $"Écrire une courte phrase avec '{phrase}'",
        Domain.Language.Russian => $"Напиши короткое предложение с '{phrase}'",
        Domain.Language.German => $"Schreibe einen kurzen Satz mit '{phrase}'",
        _ => throw new NotImplementedException()
    };

    private record Response(Choice[] choices);

    private record Choice(Message message);

    private record Message(string content);
}
