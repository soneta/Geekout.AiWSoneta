using System.Linq;
using Geekout.AiWSoneta.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using NSubstitute;
using NUnit.Framework;
using Geekout.AiWSoneta.Poczta.Abstract;
using Geekout.AiWSoneta.Poczta.Plugins;
using Geekout.AiWSoneta.Tests.SemanticKernel.Utils;
using Soneta.Core.AI.Extensions;

namespace Geekout.AiWSoneta.Tests.Poczta;

[TestFixture]
public class ProcessEmailMessagePluginTests : SemanticKernelTestBase
{
    private const string Prompt1 = """
                                   Dear Company,

                                   I hope this message finds you well.
                                   I am writing to inquire about the status of my order with the ID: ZAM-12143. Could you please provide an update on its current status and estimated delivery date?
                                   Could you please provide me with some info?

                                   Z wyrazami szacunku,
                                   Adam Nowak
                                   tel. (+48) 123-456-987
                                   adamnowak@gmailll.com
                                   """;

    private const string Prompt2 = """
                                   Dear Company,

                                   I hope this message finds you well.

                                   I am writing to inquire about the status of my orders: 1. with the ID: ZO/000001/16
                                   and the second with the ID: ZO/000001/17. 
                                   Could you please provide me with some info?

                                   Thank you for your assistance. I look forward to your prompt response.

                                   Pozdrawiam,
                                   Adam Nowak
                                   tel. (+48) 123-456-789
                                   adamnowak@gmail.com
                                   """;

    private const string Prompt3 = """
                                   Dear Company,

                                   I hope this message finds you well.

                                   I am writing to inquire about the status of all orders made by my company.
                                   Could you please provide me with some info?

                                   Thank you for your assistance. I look forward to your prompt response.

                                   Pozdrawiam,
                                   Adam Nowak
                                   tel. (+48) 123-456-789
                                   adamnowak@gmail.com
                                   """;

    private const string NonRelatedPrompt1 = """
                                             Szanowny Użytkowniku, 

                                             dziękujemy za rejestrację w bezpłatnej usłudze Onet Konto. Za jej pośrednictwem możesz zarządzać swoim profilem użytkownika (zmieniać, uzupełniać lub usuwać swoje dane), jak również założyć bezpłatną skrzynkę pocztową Onet Poczta. 

                                             Zgodnie z art. 38 pkt. 13) ustawy z dnia 30 maja 2014 o prawach konsumenta przypominamy, że udzielona przez Ciebie w dniu 2025-04-15 . o godz. 12:46:54 zgoda na rozpoczęcie świadczenia usługi przed upływem terminu na odstąpienie od umowy oznacza, że prawo do odstąpienia od usługi Onet Konto nie przysługuje Ci z chwilą aktywacji usługi. 

                                             Twoje konto jest już aktywne a usługa będzie świadczona zgodnie z obowiązującym Regulaminem (umowa na dostarczanie treści cyfrowych), który znajdziesz w załączniku. 

                                             Mamy nadzieję, że Onet Konto spełni Twoje oczekiwania. Jeżeli jednak nie będziesz chciał z niego korzystać, możesz w każdej chwili wypowiedzieć umowę/ usunąć konto, bez podania przyczyny i bez ponoszenia z tego tytułu jakichkolwiek opłat. 

                                             Pozdrawiamy,
                                             Zespół Onet Konta 

                                             """;

    private const string NonRelatedPrompt2 = """
                                             No hejka, co tam się z Tobą dzieje? Skąd to zwątpienie? Dlaczego chcesz teraz się poddać, tylko dlatego, że raz czy drugi Ci nie wyszło? To nie jest żaden powód. Musisz iść i walczyć. Osiągniesz cel. Prędzej czy później go osiągniesz, ale musisz iść do przodu, przeć, walczyć o swoje. Nie ważne, że wszystko dookoła jest przeciwko Tobie. Najważniejsze jest to, że masz tutaj wole zwycięstwa. To się liczy. Każdy może osiągnąć cel, nie ważne czy taki czy taki, ale trzeba iść i walczyć. To teraz masz trzy sekundy żeby się otrąsnąć, powiedzieć sobie "dobra basta", pięścią w stół, idę to przodu i osiągam swój cel. Pozdro.
                                             """;

    private const string FromEmailAddress = "jan.nowak@gmail.com";
    private const string ToEmailAddress = "anna.kowalska@gmail.com";
    private const string EmailTopic = "Zapytanie";

    private Kernel GetKernel(IGenerateEmailMessageService generateEmailMessageMock,
        ICommitEmailMessageService commitEmailMessageMock)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddChatCompletion(Session, ServiceAiSymbol);
        builder.Services.AddSingleton<IGenerateEmailMessageService>(x => generateEmailMessageMock);
        builder.Services.AddSingleton<ICommitEmailMessageService>(x => commitEmailMessageMock);
        builder.Plugins.AddFromType<ProcessEmailMessagePlugin>();
        return builder.Build(Session);
    }

    [TestCase(Prompt1, new[]{"ZAM-12143"})]
    [TestCase(Prompt2, new[]{ "ZO/000001/16", "ZO/000001/17" })]
    public void ProcessEmailMessagePlugin_IsInvokedWithCorrectOrderId_GivenByChatCompletionService(string prompt, string[] expectedOrderIds)
    {
        // Arrange
        var generateEmailMessageMock = GetSpeakingMockGenerateEmailMessageService();
        var commitEmailMessageMock = Substitute.For<ICommitEmailMessageService>();
        var kernel = GetKernel(generateEmailMessageMock, commitEmailMessageMock);

        // Act
        ProcesujWiadomoscEmailWorker.InvokeKernel(kernel, prompt, FromEmailAddress, ToEmailAddress,EmailTopic).GetAwaiter().GetResult();

        // Assert
        generateEmailMessageMock
            .Received(1)
            .OrdersData(Arg.Is<string[]>(x => x.SequenceEqual(expectedOrderIds)), ToEmailAddress, FromEmailAddress, EmailTopic);
        generateEmailMessageMock.DidNotReceiveWithAnyArgs().UnrecognizedTypeOfRequest(default,default, default);
        generateEmailMessageMock.DidNotReceiveWithAnyArgs().DataOfAllOrders(default, default, default);
    }

    [TestCase(Prompt3)]
    public void ProcessEmailMessagePlugin_GivenQueryForAllOrders_IsInvokedQueryAllOrders(string prompt)
    {
        // Arrange
        var generateEmailMessageMock = GetSpeakingMockGenerateEmailMessageService();
        var commitEmailMessageMock = Substitute.For<ICommitEmailMessageService>();
        var kernel = GetKernel(generateEmailMessageMock, commitEmailMessageMock);

        // Act
        ProcesujWiadomoscEmailWorker.InvokeKernel(kernel, prompt, FromEmailAddress, ToEmailAddress, EmailTopic).GetAwaiter().GetResult();

        // Assert
        generateEmailMessageMock
            .Received(1)
            .DataOfAllOrders(Arg.Is<string>(x => x.Equals(ToEmailAddress)), Arg.Is<string>(x => x.Equals(FromEmailAddress)), Arg.Is<string>(x => x.Equals(EmailTopic)));
        generateEmailMessageMock.DidNotReceiveWithAnyArgs().OrdersData(default,default,default,default);
        generateEmailMessageMock.DidNotReceiveWithAnyArgs().UnrecognizedTypeOfRequest(default,default,default);
    }

    [TestCase(NonRelatedPrompt1)]
    [TestCase(NonRelatedPrompt2)]
    public void ProcessEmailMessagePlugin_GivenNonRelatedPrompt_IsInvokedWithNonRelated(string prompt)
    {
        // Arrange
        var generateEmailMessageMock = GetSpeakingMockGenerateEmailMessageService();
        var commitEmailMessageMock = Substitute.For<ICommitEmailMessageService>();
        var kernel = GetKernel(generateEmailMessageMock, commitEmailMessageMock);

        // Act
        ProcesujWiadomoscEmailWorker.InvokeKernel(kernel, prompt, FromEmailAddress, ToEmailAddress,EmailTopic).GetAwaiter().GetResult();

        // Assert
        generateEmailMessageMock
            .Received(1)
            .UnrecognizedTypeOfRequest(Arg.Is<string>(x => x.Equals(ToEmailAddress)), 
                Arg.Is<string>(x => x.Equals(FromEmailAddress)),
                Arg.Is<string>(x => x.Equals(EmailTopic)));
        generateEmailMessageMock.DidNotReceiveWithAnyArgs().OrdersData(default,default, default, default);
        generateEmailMessageMock.DidNotReceiveWithAnyArgs().DataOfAllOrders(default,default, default);
    }

    private static IGenerateEmailMessageService GetSpeakingMockGenerateEmailMessageService()
    {
        var generateEmailMessageMock = Substitute.For<IGenerateEmailMessageService>();
        generateEmailMessageMock
            .When(x => x.DataOfAllOrders(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()))
            .Do(x => TestContext.Out.WriteLine("uruchomiono generowanie wiadomości dt. wszystkich zamówień"));
        generateEmailMessageMock
            .When(x => x.OrdersData(Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()))
            .Do(x =>
            {
                TestContext.Out.WriteLine("uruchomiono generowanie wiadomości dt. zamówień: {0}",
                    string.Join(", ", (string[])x.Args()[0]) );
            });
        generateEmailMessageMock
            .When(x => x.UnrecognizedTypeOfRequest(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()))
            .Do(x => TestContext.Out.WriteLine("generowanie wiadomości dt. nierozpoznania charakteru zapytania"));
        return generateEmailMessageMock;
    }
}
