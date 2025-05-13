using System.Threading.Tasks;
using Geekout.AiWSoneta.Poczta.Plugins;
using Geekout.AiWSoneta.Tests.SemanticKernel.Utils;
using Microsoft.SemanticKernel;
using NUnit.Framework;
using Soneta.Core.AI.Extensions;

namespace Geekout.AiWSoneta.Tests.Poczta
{
    [TestFixture]
    public class PocztaPluginTests : SemanticKernelTestBase
    {
        private const string MessageContent1 = """
                                               Drogi Kontrahencie,

                                               Mam nadzieję, że ten e-mail zastaje Cię w dobrym zdrowiu i w doskonałym nastroju. Chciałbym poświęcić chwilę, aby poinformować Cię o pewnym wydarzeniu, które miało miejsce w ostatnim czasie, a mianowicie o otrzymaniu przez nas przesyłki, którą wysłałeś. Chodzi o przesyłkę zawierającą 1000 opakowań zapałek, które były przedmiotem naszej ostatniej transakcji handlowej.
                                               Chciałbym podkreślić, że proces odbioru przesyłki przebiegł bez większych problemów, choć oczywiście, jak to bywa w takich przypadkach, zawsze mogą pojawić się pewne drobne komplikacje, które jednak nie miały większego wpływu na ogólny przebieg operacji. Przesyłka dotarła do nas w stanie nienaruszonym, co jest dla nas niezwykle istotne, biorąc pod uwagę delikatny charakter zawartości.
                                               Chciałbym również zaznaczyć, że cały proces logistyczny, począwszy od momentu nadania przesyłki, aż do jej ostatecznego odbioru, przebiegł zgodnie z naszymi oczekiwaniami. Niemniej jednak, zawsze istnieje przestrzeń do dalszej optymalizacji i doskonalenia naszych procedur, co z pewnością będziemy brać pod uwagę w przyszłości.
                                               W związku z powyższym, chciałbym jeszcze raz podziękować za współpracę i wyrazić nadzieję, że nasze przyszłe transakcje będą przebiegały równie sprawnie i bezproblemowo. Jeśli masz jakiekolwiek pytania lub wątpliwości dotyczące tej przesyłki, proszę o kontakt. Zawsze jesteśmy gotowi do udzielenia wszelkich niezbędnych informacji i wsparcia.
                                               Z wyrazami szacunku,
                                               Jan Kowalski
                                               """;

        private const string MessageTopic1 = """
                                             Otrzymanie przesyłki
                                             """;

        private const string MessageTopic2 = """
                                             Ważne informacje dotyczące projektu
                                             """;

        private const string MessageContent2 = """
                                               Drogi Zespole,
                                               Chciałbym przypomnieć, że szczegóły dotyczące naszego wewnętrznego projektu muszą pozostać poufne. Proszę nie udostępniać tych informacji kontrahentom, zwłaszcza tym, którzy nie są bezpośrednio zaangażowani w projekt.

                                               Dziękuję za zrozumienie.
                                               Pozdrawiam, Jan Kowalski
                                               """;

        [TestCase(MessageTopic1, MessageContent1)]
        [TestCase(MessageTopic2, MessageContent2)]
        public async Task Test_Email1(string testEmailTopic, string testEmailContent)
        {
            var builder = Kernel.CreateBuilder();
            builder.AddChatCompletion(Session, ServiceAiSymbol);
            var result = await PocztaPlugin.GetEmailSummary(testEmailContent, testEmailTopic,
                builder.Build(Session));
            TestContext.Out.WriteLine(result);
        }
    }
}
