using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using Geekout.AiWSoneta.Poczta.Abstract;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Handel;
using Soneta.Tools;
using Soneta.Types;

namespace Geekout.AiWSoneta.Poczta.Services;

public partial class GenerateEmailMessageService : IGenerateEmailMessageService
{
    private readonly Session _session;
    private readonly ILoggerFactory _loggerFactory;
    private ILogger _logger;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    { 
        Converters = { new JsonStringEnumConverter() },
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin,
            UnicodeRanges.LatinExtendedA, UnicodeRanges.Latin1Supplement),
        WriteIndented = true
    };

    public GenerateEmailMessageService([NotNull] Session session,
        [NotNull] ILoggerFactory loggerFactory)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }
    private static Date NowMinusThreeMonths => new(DateTime.Now.AddMonths(-6));

    private ILogger Logger => _logger ??= _loggerFactory.CreateLogger(nameof(GenerateEmailMessageService));

    public WiadomoscEmail DataOfAllOrders(string toAddress, string fromAddress, string msgTopic)
    {
        WiadomoscEmail result;
        try
        {
            result = DoGetDataOfAllOrders(toAddress,fromAddress, msgTopic);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warning,
                "Błąd podczas generowania wiadomości dt. wszystkich zamówień: {0}".Translate(), ex.Message);
            return null;
        }
        LogGeneratedContent(result);
        return result;
    }

    public WiadomoscEmail UnrecognizedTypeOfRequest(string toAddress, string fromAddress, string msgTopic)
    {
        WiadomoscEmail result;
        try
        {
            result = GetUnrecognizedMessage(toAddress, fromAddress, msgTopic);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warning,
                "Błąd podczas generowania wiadomości dt. nierozpoznania charakteru zapytania: {0}".Translate(), ex.Message);
            return null;
        }
        LogGeneratedContent(result);
        return result;
    }

    public WiadomoscEmail OrdersData(string[] numeryZamowienia, string toAddress, string fromAddress, string msgTopic)
    {
        WiadomoscEmail result;
        try
        {
            if (numeryZamowienia == null || numeryZamowienia.Length == 0)
                return null;

            result = DoGetOrdersData(numeryZamowienia, toAddress, fromAddress, msgTopic);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warning, "Błąd podczas generowania wiadomości dt. zamówienia: {0}".Translate(), ex.Message);
            return null;
        }
        LogGeneratedContent(result);
        return result;
    }

    private void LogGeneratedContent(WiadomoscEmail result)
    {
        if (result == null) return;
        Logger.Log(LogLevel.Information, "Wygenerowano następującą treść: {0}".Translate(), result.Tresc);
    }

    private WiadomoscEmail DoGetDataOfAllOrders(string toAddress, string fromAddress, string msgTopic)
    {
        var orders = GetOrdersByContact(fromAddress)?
            .Where(IsValidOrder).Select(dokHandlowy => new OrderInfo(dokHandlowy)).ToArray();
        if (orders == null || orders.Length == 0) return null;

        return SendResponseEveryOrder(JsonSerializer.Serialize(orders, _jsonSerializerOptions), toAddress, fromAddress, msgTopic);
    }

    private WiadomoscRobocza DoGetOrdersData(string[] numeryZamowienia, string toAddress, string fromAddress,  string msgTopic)
    {
        var orders = GetOrdersByContact(fromAddress);
        var orderInfos = numeryZamowienia.Select(numerZamowienia =>
        {
            var orderWithNumerZamowienia = orders
                .FirstOrDefault(dokHandlowy => (dokHandlowy.NumerPelnyZapisany == numerZamowienia 
                                                || dokHandlowy.Obcy.Numer == numerZamowienia)
                                               && IsValidOrder(dokHandlowy));
            return orderWithNumerZamowienia is null
                ? null
                : new OrderInfo(orderWithNumerZamowienia);
        }).Where(x => x is not null).ToArray();
        if (orderInfos.Length == 0) return null;

        return GetResponseForOrderId(numeryZamowienia, JsonSerializer.Serialize(orderInfos, _jsonSerializerOptions), toAddress, fromAddress, msgTopic);
    }

    private static bool IsValidOrder(DokumentHandlowy dokHandlowy) 
        => dokHandlowy.Dostawa.Termin.CompareTo(NowMinusThreeMonths) > 0;

    private IEnumerable<DokumentHandlowy> GetOrdersByEmailAddress(string emailAddress)
    {
        var crmModule = CRMModule.GetInstance(_session);

        var resultSearchForKontakt = crmModule.KontaktyOsoby.Rows.Cast<KontaktOsoba>()
            .FirstOrDefault(x => x.EMAIL == emailAddress);

        if (resultSearchForKontakt == null)
            return null;

        var resultKontrahenci =
            crmModule.OsobyKontrahenci.WgOsobaKontaktowa[resultSearchForKontakt]
                .Where(x => x.Kontrahent is Kontrahent)
                .Select(x => x.Kontrahent as Kontrahent);
        var handelModule = HandelModule.GetInstance(_session);
        return resultKontrahenci.SelectMany(kontrahent => handelModule.DokHandlowe.WgKontrahent[kontrahent])
            .Where(x => x.Kategoria == KategoriaHandlowa.ZamówienieOdbiorcy);
    }

    private IEnumerable<DokumentHandlowy> GetOrdersByContact(string fromAddress)
    {
        var adresEmail = ExtractEmailAddress(fromAddress);
        return GetOrdersByEmailAddress(adresEmail);
    }

    private static string ExtractEmailAddress(string fromAddress)
    {
        var validateEmailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
        return validateEmailRegex.Match(fromAddress).Value;
    }
}
