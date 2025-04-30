using System.Linq;
using Geekout.AiWSoneta.Poczta.Plugins;
using Geekout.AiWSoneta.UI;
using Microsoft.SemanticKernel;
using Soneta.Business;
using Soneta.Core;
using Soneta.Core.AI.Extensions;
using Soneta.CRM;
using Soneta.Tools;

[assembly: Worker(typeof(StreszczenieWiadomosciWorker),
    typeof(WiadomoscEmail)
)]

namespace Geekout.AiWSoneta.UI;

public class StreszczenieWiadomosciWorker
{

    private Log _log;
    private Log Log => _log ??= new Log(nameof(StreszczenieWiadomosciWorker), true);

    [Context]
    public StreszczenieParams Params { get; set; }

    [Context]
    public WiadomoscEmail WiadomoscEmail { get; set; }

    [Action("Wygeneruj streszczenie", Target = ActionTarget.Menu, Mode = ActionMode.SingleSession, Priority = 2)]
    public void GetSummary()
    {
        var trescWiadomosci = WiadomoscEmail.Tresc.ToString();
        var tematWiadomosci = WiadomoscEmail.Temat;
        var id = WiadomoscEmail.ID;
        var result = PocztaPlugin.GetEmailSummary(trescWiadomosci, tematWiadomosci, GetKernel()).Result;
        Log.WriteLine("Streszczenie dla wiadomości o ID={0}".Translate(), id);
        Log.WriteLine("Temat wiadomości: {0}".Translate(), tematWiadomosci);
        Log.WriteLine(string.Concat(Enumerable.Repeat("-",60)));
        Log.WriteLine(result);
    }

    private Kernel GetKernel()
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddChatCompletion(Params.SystemZewn as SystemZewnSerwisAI);
        return kernelBuilder.Build();
    }
}
