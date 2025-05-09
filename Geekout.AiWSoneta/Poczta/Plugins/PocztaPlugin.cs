using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace Geekout.AiWSoneta.Poczta.Plugins;

public class PocztaPlugin
{
    [Description(
        "Zwraca streszczenie wiadomości e-mail na podstawie tematu i treści wiadomości")]
    public static Task<string> GetEmailSummary(
        [Description("TREŚĆ wiadomości e-mail")]
        string emailContents,
        [Description("TEMAT wiadomości")]
        string emailTopic,
        Kernel kernel)
    {
        var function = GetSummaryFunction(kernel);
        var args = new KernelArguments
        {
            [nameof(emailContents)] = emailContents,
            [nameof(emailTopic)] = emailTopic
        };
        return kernel.InvokeAsync(function, args).ContinueWith(task => task.Result.GetValue<string>());
    }

    private static KernelFunction GetSummaryFunction(Kernel kernel)
    {
        var config = new PromptTemplateConfig
        {
            Template = """
                       Utwórz streszczenie wiadomości e-mail, której treść znajdziesz poniżej:
                       '{{$emailContents}}'
                       Streszczenie musi być co najwyżej dwuzdaniowe. W odpowiedzi zawrzyj jedynie treść streszczenia.
                       """
        };

        var function = kernel.CreateFunctionFromPrompt(config);
        return function;
    }

}
