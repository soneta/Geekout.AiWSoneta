using Soneta.CRM;

namespace Geekout.AiWSoneta.Poczta.Abstract;

public interface IGenerateEmailMessageService
{
    WiadomoscEmail OrdersData(string[] numeryZamowienia, string toAddress, string fromAddress, string msgTopic);
    WiadomoscEmail DataOfAllOrders(string toAddress, string fromAddress, string msgTopic);
    WiadomoscEmail UnrecognizedTypeOfRequest(string toAddress, string fromAddress, string msgTopic);
}
