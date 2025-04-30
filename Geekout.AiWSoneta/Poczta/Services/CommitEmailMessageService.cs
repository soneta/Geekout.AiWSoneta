using System;
using Geekout.AiWSoneta.Poczta.Abstract;
using Geekout.AiWSoneta.Poczta.Plugins;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Tools;

namespace Geekout.AiWSoneta.Poczta.Services;

public class CommitEmailMessageService : ICommitEmailMessageService
{
    private readonly Session _session;
    private readonly ILoggerFactory _loggerFactory;
    private ILogger _logger;

    public CommitEmailMessageService([NotNull] Session session, ILoggerFactory loggerFactory)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _loggerFactory = loggerFactory;
    }
    private ILogger Logger => _logger ??= _loggerFactory.CreateLogger(nameof(CommitEmailMessageService));

    public EmailMessageResponse CommitMail(WiadomoscEmail resultMail)
    {
        try
        {
            if (resultMail is null) return null;
            using var trans = _session.Logout(true);
            CRMModule.GetInstance(_session).WiadomosciEmail.AddRow(resultMail);
            trans.Commit();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warning, "Błąd podczas próby zapisu wiadomości email: {0}".Translate(), ex.Message);
            return new EmailMessageResponse(resultMail, false);
        }

        return new EmailMessageResponse(resultMail, true);
    }
}
