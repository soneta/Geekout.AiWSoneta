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
        var functionResult = InvokeKernel(BuildKernel(), WiadomoscEmail.Tresc, WiadomoscEmail.Od, 
            WiadomoscEmail.Do, WiadomoscEmail.Temat).GetAwaiter().GetResult();
    }

    internal static Task<FunctionResult> InvokeKernel(Kernel kernel, string tresc, string nadawca, string odbiorca, string temat)
    {
        var options = new FunctionChoiceBehaviorOptions
        { AllowConcurrentInvocation = false, AllowParallelCalls = false };
        var settings = new OpenAIPromptExecutionSettings
        { FunctionChoiceBehavior = FunctionChoiceBehavior.Required(options: options) };
        var promptTemplate = GetKernelFunction(kernel);
        var functionResult = kernel.InvokeAsync(promptTemplate,
            new KernelArguments(settings)
            {
                ["emailMessage"] = tresc,
                ["toAddress"] = odbiorca,
                ["fromAddress"] = nadawca,
                ["msgTopic"] = temat
            }
        );
        return functionResult;
    }

    private static KernelFunction GetKernelFunction(Kernel kernel)
    {
        var promptTemplate = kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig("""
                                     Poniżej znajduje się wiadomość e-mail od użytkownika. 
                                     Jeśli to możliwe, przetwórz ją zgodnie z intencją użytkownika.
                                     Temat wiadomości: {{$msgTopic}}
                                     Adres nadawcy: {{$fromAddress}}
                                     Adres odbiorcy: {{$toAddress}}
                                     Wiadomość e-mail: {{$emailMessage}}
                                     """.TranslateIgnore()));
        return promptTemplate;
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
