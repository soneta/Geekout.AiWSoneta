using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Geekout.AiWSoneta.Poczta.Abstract;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Handel;
using Soneta.Tools;

namespace Geekout.AiWSoneta.Poczta.Services;

public partial class GenerateEmailMessageService : IGenerateEmailMessageService
{
    private readonly Session _session;
    private readonly ILoggerFactory _loggerFactory;
    private ILogger _logger;

    public GenerateEmailMessageService([NotNull] Session session,
        [NotNull] ILoggerFactory loggerFactory)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    private ILogger Logger => _logger ??= _loggerFactory.CreateLogger(nameof(GenerateEmailMessageService));

    public WiadomoscEmail DataOfAllOrders(string fromAddress, string msgTopic)
    {
        WiadomoscEmail result;
        try
        {
            result = DoGetDataOfAllOrders(fromAddress, msgTopic);
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

    public WiadomoscEmail UnrecognizedTypeOfRequest(string fromAddress, string msgTopic)
    {
        WiadomoscEmail result;
        try
        {
            result = GenerateEmailMessageService.GetUnrecognizedMessage(fromAddress, msgTopic);
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

    public WiadomoscEmail OrdersData(string[] numeryZamowienia, string fromAddress, string msgTopic)
    {
        WiadomoscEmail result;
        try
        {
            if (numeryZamowienia == null || numeryZamowienia.Length == 0)
                return null;

            result = DoGetOrdersData(numeryZamowienia, fromAddress, msgTopic);
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

    private WiadomoscEmail DoGetDataOfAllOrders(string fromAddress, string msgTopic)
    {
        var orders = GetOrdersByContact(fromAddress)?
            .Select(x => new GenerateEmailMessageService.OrderInfo(x)).ToArray();
        if (orders == null || orders.Length == 0) return null;

        return GenerateEmailMessageService.SendResponseEveryOrder(JsonSerializer.Serialize(orders,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() }, WriteIndented = true }), fromAddress, msgTopic);
    }

    private WiadomoscRobocza DoGetOrdersData(string[] numeryZamowienia, string fromAddress, string msgTopic)
    {
        var orders = GetOrdersByContact(fromAddress);
        var orderInfos = numeryZamowienia.Select(numerZamowienia =>
        {
            var orderWithNumerZamowienia = orders
                .FirstOrDefault(dokHandlowy => dokHandlowy.NumerPelnyZapisany == numerZamowienia
                                               || dokHandlowy.Obcy.Numer == numerZamowienia);
            return orderWithNumerZamowienia is null
                ? null
                : new GenerateEmailMessageService.OrderInfo(orderWithNumerZamowienia);
        }).Where(x => x is not null).ToArray();
        if (orderInfos.Length == 0) return null;

        return GenerateEmailMessageService.GetResponseForOrderId(numeryZamowienia, JsonSerializer.Serialize(orderInfos,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() }, WriteIndented = true }
        ), fromAddress, msgTopic);
    }

    private IEnumerable<DokumentHandlowy> GetOrdersByEmailAddress(string adresEmail)
    {
        var crmModule = CRMModule.GetInstance(_session);

        var resultSearchForKontakt = crmModule.KontaktyOsoby.Rows.Cast<KontaktOsoba>()
            .FirstOrDefault(x => x.EMAIL == adresEmail);

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
