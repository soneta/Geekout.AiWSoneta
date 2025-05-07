using System;
using System.ComponentModel;
using Geekout.AiWSoneta.Poczta.Abstract;
using JetBrains.Annotations;
using Microsoft.SemanticKernel;
using Soneta.CRM;

namespace Geekout.AiWSoneta.Poczta.Plugins;

public class ProcessEmailMessagePlugin
{
    private readonly IGenerateEmailMessageService _generateMailService;
    private readonly ICommitEmailMessageService _commitMailService;

    public ProcessEmailMessagePlugin([NotNull] IGenerateEmailMessageService plugin,
        [NotNull] ICommitEmailMessageService commitEmailMessageService)
    {
        _generateMailService = plugin ?? throw new ArgumentNullException(nameof(plugin));
        _commitMailService = commitEmailMessageService ?? throw new ArgumentNullException(nameof(commitEmailMessageService));
    }

    [Description("Odpowiada na zapytanie o informacje o zamówieniu, zwracając je na podstawie NUMERÓW ZAMÓWIENIA w zapytaniu")]
    [KernelFunction]
    public EmailMessageResponse OrdersData([Description("NUMERY ZAMÓWIENIA z zapytania użytkownika")] string[] numeryZamowienia,
        [Description("Adres odbiorcy wiadomości email")] string toAddress,
        [Description("Adres nadawcy wiadomości email")] string fromAddress,
        [Description("Temat wiadomości email")] string msgTopic)
    {
        var email = _generateMailService.OrdersData(numeryZamowienia, toAddress,fromAddress, msgTopic);
        return _commitMailService.CommitMail(email);
    }

    [Description("Odpowiada na zapytanie o informacje o zamówienia, zwracając w wiadomości dane wszystkich zamówień na podstawie danych nadawcy")]
    [KernelFunction]
    public EmailMessageResponse DataOfAllOrders(
        [Description("Adres odbiorcy wiadomości email")] string toAddress,
        [Description("Adres nadawcy wiadomości email")] string fromAddress,
        [Description("Temat wiadomości email")] string msgTopic)
    {
        var email = _generateMailService.DataOfAllOrders(toAddress,fromAddress, msgTopic);
        return _commitMailService.CommitMail(email);
    }

    [Description("Odpowiada nadawcy w przypadku zadania nierozpoznanego typu zapytania")]
    [KernelFunction]
    public EmailMessageResponse UnrecognizedTypeOfRequest(
        [Description("Adres odbiorcy wiadomości email")] string toAddress,
        [Description("Adres nadawcy wiadomości email")] string fromAddress,
        [Description("Temat wiadomości email")] string msgTopic)
    {
        var email = _generateMailService.UnrecognizedTypeOfRequest(toAddress, fromAddress, msgTopic);
        return _commitMailService.CommitMail(email);
    }
}
public record EmailMessageResponse(string Message, string Topic, string Recipient, bool WasCommitSuccessful)
{
    public EmailMessageResponse(WiadomoscEmail email, bool wasCommitSuccessful)
        : this(email.Tresc, email.Temat, email.Do, wasCommitSuccessful) { }
}


