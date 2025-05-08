using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geekout.AiWSoneta.Tests.RAG.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using NUnit.Framework;
using Soneta.Core.AI.Extensions;

namespace Geekout.AiWSoneta.Tests.RAG;

[Experimental("SKEXP0001")]
public class VectorSearchWithFunctionCallingAndAiTest : RagTestBase
{
    /// <summary>
    /// Test demonstrujący wyszukiwanie wektorowe pracowników i generowanie odpowiedzi AI.
    /// Wykonuje wyszukiwanie semantyczne na podstawie zapytania użytkownika, przygotowuje kontekst
    /// z danymi pracowników i wywołuje model AI z funkcją automatycznego wywoływania funkcji kernelowych.
    /// </summary>
    [Test]
    [TestCase("Wypisz mi wszystkie osoby, które ukończyły jakąkolwiek szkołę wyższą / studia")]
    [TestCase("Wypisz mi wszystkie osoby, które ukończyły jakąkolwiek szkołę wyższą / studia z wydziału 'adm'")]
    [TestCase("Wypisz mi wszystkie osoby, które ukończyły jakąkolwiek szkołę wyższą / studia z wydziału 'EC salon'")]
    public async Task Answer(string query)
    {
        // Utworzenie kernela
        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging(loggingBuilder =>
            loggingBuilder.AddDebug()
                .SetMinimumLevel(LogLevel.Trace));
        builder.AddChatCompletion(Session, ServiceAiSymbol);
        // builder.AddAiService(Session, ServiceAiSymbol,
        //     (kernelBuilder, ai) => kernelBuilder.AddAzureOpenAIChatCompletion(
        //         "gpt-4.1-mini",
        //         ai.Config.Serwer,
        //         ai.Config.KluczApi));
        builder.Plugins.AddFromType<PracownicyPlugin>();
        var kernel = builder.Build(Session);

        // Wektoryzacja zapytania
        var embeddings = await EmbeddingService.GenerateEmbeddingAsync(query);
        
        // Wyszukiwanie wektorowe - 5 najlepszych wyników
        var searchResults = await VectorStoreCollection.VectorizedSearchAsync(
            embeddings,
            new () { Top = 5 });
        var results = searchResults.Results.ToBlockingEnumerable().ToArray();

        var employeesContext = new StringBuilder();
        foreach (var item in results)
        {
            // Dodanie pracownika do kontekstu dla modelu AI
            employeesContext.AppendLine($"Kod: {item.Record.Kod} Imię: {item.Record.Imie}, Nazwisko: {item.Record.Nazwisko}");
            employeesContext.AppendLine($"Doświadczenie: {item.Record.Doswiadczenie}");
            employeesContext.AppendLine();
        }
        
        // Przygotowanie argumentów dla modelu AI
        var args = new KernelArguments(
            new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        {
            { "employees", employeesContext.ToString() },
            { "query", query }
        };
        
        // Prompt dla modelu AI z wyszukanymi pracownikami jako kontekst
        var prompt = 
            """
             Jeżeli w sekcji zapytanie użytkownika występuje wskazanie wydziału, 
              pobierz listę kodów pracowników tego wydziału.
             Na podstawie zapytania użytkownika i dostępnych informacji o pracownikach w sekcji Pracownicy przeanalizuj,
              którzy pracownicy spełniają podane kryteria.
             Zwróć uwagę na wszystkie istotne cechy, takie jak wykształcenie, doświadczenie, umiejętności, wydział 
              w zależności od treści zapytania.
             Wynik przedstaw w zwięzłej, numerowanej liście zawierającej:
             - Imię i nazwisko oraz kod pracownika
             - Szczegóły istotne dla zapytania (np. wykształcenie, doświadczenie)
             Ignoruj pracowników, którzy nie spełniają kryteriów zapytania.
             

             Zapytanie użytkownika: 
             {{$query}}

             Pracownicy:
             {{$employees}}

             Odpowiedź:
             """;

        // Wywołanie modelu AI z wyszukanymi pracownikami jako kontekst
        var aiResponse = await kernel.InvokePromptAsync(prompt, args);
        
        // Wyświetlenie odpowiedzi modelu AI
        aiResponse.ToTestOutput();
    }
}