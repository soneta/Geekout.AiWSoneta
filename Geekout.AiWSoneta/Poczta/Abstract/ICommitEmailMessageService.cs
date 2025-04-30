using Geekout.AiWSoneta.Poczta.Plugins;
using Soneta.CRM;

namespace Geekout.AiWSoneta.Poczta.Abstract;

public interface ICommitEmailMessageService
{
    EmailMessageResponse CommitMail(WiadomoscEmail resultMail);
}
