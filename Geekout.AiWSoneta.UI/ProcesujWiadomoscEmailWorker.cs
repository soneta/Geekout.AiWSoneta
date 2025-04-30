using System.Threading.Tasks;
using Geekout.AiWSoneta.Poczta.Abstract;
using Geekout.AiWSoneta.Poczta.Plugins;
using Geekout.AiWSoneta.Poczta.Services;
using Geekout.AiWSoneta.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Soneta.Business;
using Soneta.Core;
using Soneta.Core.AI.Extensions;
using Soneta.CRM;
using Soneta.Tools;

[assembly: Worker(typeof(ProcesujWiadomoscEmailWorker),
    typeof(WiadomoscEmail)
)]
namespace Geekout.AiWSoneta.UI;

public class ProcesujWiadomoscEmailWorker
{
    [Context]
    public StreszczenieParams Params { get; set; }

    [Context]
    public WiadomoscEmail WiadomoscEmail { get; set; }

    [Context]
    public Session Session { get; set; }

    private Log _log;
    private Log Log => _log ??= new Log(nameof(ProcesujWiadomoscEmailWorker), true);

    [Action("Procesuj wiadomość", Target = ActionTarget.Menu, Mode = ActionMode.SingleSession, Priority = 3)]
    public void Worker()
    {
        var functionResult = InvokeKernel(BuildKernel(), WiadomoscEmail.Tresc, WiadomoscEmail.Od, WiadomoscEmail.Temat).GetAwaiter().GetResult();
    }

    internal static Task<FunctionResult> InvokeKernel(Kernel kernel, string tresc, string od, string temat)
    {
        var options = new FunctionChoiceBehaviorOptions
        { AllowConcurrentInvocation = false, AllowParallelCalls = false };
        var settings = new OpenAIPromptExecutionSettings
        { FunctionChoiceBehavior = FunctionChoiceBehavior.Required(options: options) };
        var promptTemplate = kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig("""
            Poniżej znajduje się wiadomość e-mail od użytkownika. 
            Jeśli to możliwe, przetwórz ją zgodnie z intencją użytkownika.
            Temat wiadomości: {{$msgTopic}}
            Adres nadawcy: {{$fromAddress}}
            Wiadomość e-mail: {{$emailMessage}}
            """.TranslateIgnore()));
        var functionResult = kernel.InvokeAsync(promptTemplate,
            new KernelArguments(settings)
            {
                ["emailMessage"] = tresc,
                ["fromAddress"] = od,
                ["msgTopic"] = temat
            }
        );
        return functionResult;
    }

    private Kernel BuildKernel()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddChatCompletion(Params.SystemZewn as SystemZewnSerwisAI);
        builder.Services.AddSingleton<IGenerateEmailMessageService, GenerateEmailMessageService>();
        builder.Services.AddSingleton<ICommitEmailMessageService, CommitEmailMessageService>();
        builder.Plugins.AddFromType<ProcessEmailMessagePlugin>();
        return builder.Build(Session);
    }
}
