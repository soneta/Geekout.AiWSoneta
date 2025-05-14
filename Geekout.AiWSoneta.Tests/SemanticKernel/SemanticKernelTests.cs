using System.Threading.Tasks;
using Geekout.AiWSoneta.Tests.RAG.Utils;
using Geekout.AiWSoneta.Tests.SemanticKernel.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using NUnit.Framework;
using Soneta.Business;
using Soneta.Business.App;
using Soneta.Core;
using Soneta.Core.AI.Extensions;
using Soneta.Test.Helpers;

namespace Geekout.AiWSoneta.Tests.SemanticKernel;

public class SemanticKernelTests : SemanticKernelTestBase
{
    /// <summary>
    /// Przykład użycia Semantic Kernel z Azure OpenAI bez użycia rozszerzeń Soneta.
    /// Uwagę zwraca brak wiedzy modelu na temat poruszony w prompt.
    /// </summary>
    [Test]
    public async Task Create_Kernel()
    {
        // Tworzymy instancję builder'a
        var builder = Kernel.CreateBuilder();
        
        // Dodajemy usługi generowania tekstu modelu Azure Open AI z konfiguracji testów,
        // które są pozyskiwane z user secrets
        builder.AddAzureOpenAIChatCompletion(
            AIConfiguration.AzureOpenAI.DeploymentName,
            AIConfiguration.AzureOpenAI.Endpoint,
            AIConfiguration.AzureOpenAI.ApiKey);

        // Budujemy instancję kernela
        var kernel = builder.Build();
        
        // Wykonujemy zapytanie do modelu poprzez metodę kernela
        var result = await kernel.InvokePromptAsync("Witaj na GeekOut 2025, czy wiesz co to za impreza?");
        result.ToTestOutput();
        
        // Alternatywnie można użyć usługi IChatCompletionService
        var chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();
        var chatMessageContents = await chatCompletionService.GetChatMessageContentsAsync(
            new ChatHistory(
            [new ChatMessageContent(
                AuthorRole.User, "Witaj na GeekOut 2025, czy wiesz co to za impreza?")]));
        chatMessageContents.ToTestOutput();
        
        // Alternatywnie można użyć usługi ITextGenerationService
        var textGenerationService = kernel.Services.GetRequiredService<ITextGenerationService>();
        var textContents = await textGenerationService.GetTextContentsAsync(
            "Witaj na GeekOut 2025, czy wiesz co to za impreza?");
        textContents.ToTestOutput();
    }

    /// <summary>
    /// Przykład użycia Semantic Kernel z Azure OpenAI z użyciem obiektu biznesowego SystemZewnetrzny.
    /// </summary>
    [Test]
    public void Create_Kernel_With_ExternalSystem()
    {
        // Pobieramy instancję systemu zewnętrznego z zapisanego w bazie danych
        var aiService = Session.GetCore().SystemyZewn.WgSymbol[ServiceAiSymbol];
        var builder = Kernel.CreateBuilder();
        
        // Dodajemy usługi generowania tekstu modelu Azure Open AI z systemu zewnętrznego
        builder.AddAzureOpenAIChatCompletion(
            aiService.Config.NrKlienta,
            aiService.Config.Serwer,
            aiService.Config.KluczApi);
        var kernel = builder.Build();

        
        // Tak jak w poprzednim teście mamy do dyspozycji wszystkie serwisy i metody kernela np IChatCompletionService
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }
    
    [Test]
    public async Task Create_Kernel_With_ExternalSystem_And_Extensions()
    {
        var builder = Kernel.CreateBuilder();
        // Dodajemy usługi generowania tekstu modelu Azure Open AI za pomocą rozszerzenia Soneta,
        // bez potrzeby wyszukiwania systemu zewnętrznego, ani przepisywania konfiguracji
        builder.AddChatCompletion(Session, ServiceAiSymbol);
        // Dodajemy plugin, który ma zależności biznesowe
        builder.Plugins.AddFromType<SamplePlugin>();
        var kernel = builder.Build(Session);

        // Standardowo mamy do dyspozycji wszystkie serwisy i metody kernela np IChatCompletionService
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        // Dodatkowo mamy dostęp do wszystkich serwisów biznesowych z jego różnych scope'ów
        var database = kernel.GetRequiredService<Database>();
        var login = kernel.GetRequiredService<Login>();
        var session = kernel.GetRequiredService<Session>();
        var budgetService = kernel.GetRequiredService<IBudgetService>();

        // Kernel potrafi utworzyć instancję pluginu, który ma zależności biznesowe
        var samplePlugin = kernel.Plugins[nameof(SamplePlugin)];
        var sampleFunction = samplePlugin[nameof(SamplePlugin.GetSampleText)];
        
        // Możemy wywołać funkcję pluginu, która ma zależności biznesowe
        var value = await kernel.InvokeAsync(sampleFunction);
        value.ToTestOutput();
    }
}