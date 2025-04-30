using Soneta.CRM;

namespace Geekout.AiWSoneta.Poczta.Abstract;

public interface IGenerateEmailMessageService
{
    WiadomoscEmail OrdersData(string[] numeryZamowienia, string fromAddress, string msgTopic);
    WiadomoscEmail DataOfAllOrders(string fromAddress, string msgTopic);
    WiadomoscEmail UnrecognizedTypeOfRequest(string fromAddress, string msgTopic);
}
