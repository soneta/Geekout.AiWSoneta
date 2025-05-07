using System;
using System.Linq;
using Soneta.CRM;
using Soneta.Tools;

namespace Geekout.AiWSoneta.Poczta.Services;

public partial class GenerateEmailMessageService
{
    private static WiadomoscRobocza GetResponseForOrderId(string[] numeryZamowien, string infoForResponse,
        string toAddress, string fromAddress, string msgTopic)
    {
        var message = numeryZamowien.Length > 1
            ? """
              Nawiązując do poprzedniej wiadomości, przesyłam informacje dotyczące zamówień nr: {0}.
              Informacje: {1}
              """.TranslateFormat(string.Join(", ", numeryZamowien), infoForResponse)
            : """
              Nawiązując do poprzedniej wiadomości, przesyłam informacje dotyczące zamówienia nr: {0}.
              Informacje: {1}
              """.TranslateFormat(numeryZamowien.Single(), infoForResponse);

        return new WiadomoscRobocza
        {
            Data = DateTime.Now,
            Tresc = message,
            Od = toAddress,
            Do = fromAddress,
            Temat = "[ODP] {0}".TranslateFormat(msgTopic)
        };
    }

    private static WiadomoscRobocza GetUnrecognizedMessage(string toAddress, string fromAddress, string msgTopic)
    {
        var message = """
                      Niestety, nie jestem w stanie automatycznie przetworzyć tej wiadomości. Spróbuj odpowiedzieć na nią ręcznie.
                      """.Translate();

        return new WiadomoscRobocza
        {
            Data = DateTime.Now,
            Tresc = message,
            Od = toAddress,
            Do = fromAddress,
            Temat = "[ODP][NIEUDANY] {0}".TranslateFormat(msgTopic)
        };
    }

    private static WiadomoscRobocza SendResponseEveryOrder(string infoForResponse, string toAddress, string fromAddress, string msgTopic)
    {
        var message = """
                      Nawiązując do poprzedniej wiadomości, przesyłam informacje dotyczące zamówień dla kontrahenta skojarzonego z twoim adresem email.
                      Informacje: {0}
                      """.TranslateFormat(infoForResponse);

        return new WiadomoscRobocza
        {
            Data = DateTime.Now,
            Tresc = message,
            Od = toAddress,
            Do = fromAddress,
            Temat = "[ODP] {0}".TranslateFormat(msgTopic)
        };
    }
}
