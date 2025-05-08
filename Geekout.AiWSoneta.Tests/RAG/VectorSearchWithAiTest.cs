using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geekout.AiWSoneta.Tests.RAG.Utils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using NUnit.Framework;
using Soneta.Core.AI.Extensions;

namespace Geekout.AiWSoneta.Tests.RAG;

/// <summary>
/// Test demonstrujący wyszukiwanie wektorowe na danych pracowników
/// </summary>
[Experimental("SKEXP0001")]
public class VectorSearchWithAiTest : RagTestBase
{
    
    /// <summary>
    /// Test demonstrujący wyszukiwanie wektorowe pracowników i generowanie odpowiedzi AI
    /// </summary>
    [Test]
    [TestCase("Wypisz mi wszystkie osoby, które ukończyły jakąkolwiek szkołę wyższą / studia")]
    public async Task Answer(string query)
    {
        // Utworzenie kernela
        var builder = Kernel.CreateBuilder();
        builder.AddChatCompletion(Session, ServiceAiSymbol);
        var kernel = builder.Build(Session);

        // Wektoryzacja zapytania
        var embeddings = await EmbeddingService.GenerateEmbeddingAsync(query);
        
        // Wyszukiwanie wektorowe - 5 najlepszych wyników
        var searchResults = await VectorStoreCollection.VectorizedSearchAsync(
            embeddings,
            new () { Top = 5 });
        var results = searchResults.Results.ToBlockingEnumerable().ToArray();
        
        // Przygotowanie kontekstu dla modelu AI
        var employeesContext = new StringBuilder();
        foreach (var item in results)
        {
            // Dodanie pracownika do kontekstu dla modelu AI
            employeesContext.AppendLine($"Kod: {item.Record.Kod} Imię: {item.Record.Imie}, Nazwisko: {item.Record.Nazwisko}");
            employeesContext.AppendLine($"Doświadczenie: {item.Record.Doswiadczenie}");
            employeesContext.AppendLine();
        }
        
        // Przygotowanie argumentów dla modelu AI
        var args = new KernelArguments
        {
            { "employees", employeesContext.ToString() },
            { "query", query }
        };
        
        // Prompt dla modelu AI z wyszukanymi pracownikami jako kontekst
        var prompt = 
            """
             Na podstawie zapytania użytkownika i dostępnych informacji o pracownikach, przeanalizuj, którzy pracownicy spełniają podane kryteria.
             Zwróć uwagę na wszystkie istotne cechy, takie jak wykształcenie, doświadczenie czy umiejętności, w zależności od treści zapytania.
             Wynik przedstaw w zwięzłej, numerowanej liście zawierającej:
             - Imię i nazwisko
             - Szczegóły istotne dla zapytania (np. wykształcenie, doświadczenie)
             Ignoruj pracowników, którzy nie spełniają kryteriów zapytania.

             Zapytanie: {{$query}}

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