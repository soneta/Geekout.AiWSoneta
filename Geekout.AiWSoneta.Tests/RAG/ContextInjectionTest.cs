using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Geekout.AiWSoneta.Tests.RAG.Utils;
using Microsoft.SemanticKernel;
using NUnit.Framework;
using Soneta.Core.AI.Extensions;
using Soneta.Kadry;

namespace Geekout.AiWSoneta.Tests.RAG;

/// <summary>
/// Test demonstrujący wstrzyknięcie dokumentu do kontekstu prompta i wygenerowanie podsumowania
/// </summary>
[Experimental("SKEXP0010")]
public class ContextInjectionTest : RagTestBase
{
    
    /// <summary>
    /// Test demonstrujący wstrzyknięcie informacji do kontekstu prompta i wygenerowanie podsumowania
    /// </summary>
    [Test]
    public async Task InjectInformationIntoContextAndSummarize()
    {
        // Utworzenie kernela
        var builder = Kernel.CreateBuilder();
        builder.AddChatCompletion(Session, ServiceAiSymbol);
        var kernel = builder.Build(Session);
        
        // Pobranie informacji o doświadczeniu pracownika
        var employeeData = Session.GetKadry().Pracownicy.WgKodu["006"].ToEmployeeData();
        
        // Przygotowanie argumentów dla kernela
        var args = new KernelArguments
        {
            { "doc", employeeData.Doswiadczenie }
        };
        
        // Prompt do streszczenia dokumentu
        const string summarizePrompt = 
           """
           Napisz bardzo krótkie, profesjonalne podsumowanie dołączonych informacji o doświadczeniu zawodowym pracownika.
           Podsumowanie powinno być w podpunktach i zawierać:
           1. Wykształcenie
           2. Poprzednie miejsca pracy
           3. Główne obowiązki i umiejętności
           4. Lata doświadczenia

           Informacje:
           {{$doc}}

           Podsumowanie:
           """;
        
        // Wywołanie modelu z promptem zawierającym wstrzyknięty dokument
        var result = await kernel.InvokePromptAsync(summarizePrompt, args);
        result.ToTestOutput();
    }
}