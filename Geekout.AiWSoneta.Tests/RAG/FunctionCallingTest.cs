using System.Threading.Tasks;
using Geekout.AiWSoneta.Tests.RAG.Utils;
using Geekout.AiWSoneta.Tests.SemanticKernel.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NUnit.Framework;
using Soneta.Core.AI.Extensions;

namespace Geekout.AiWSoneta.Tests.RAG;

/// <summary>
/// Test demonstrujący Function Calling
/// </summary>
public class FunctionCallingTest : SemanticKernelTestBase
{

   [Test]
   [TestCase("Podaj listę pracowników wydziału 'Adm'")]
   [TestCase("Podaj listę pracowników wydziału 'EC salon'")]
   [TestCase("Podaj listę pracowników")]
   [TestCase("Podaj listę kontrahentów")]
   public async Task GetEmployeesList(string prompt)
   {
      // Utworzenie kernela oraz dodanie pluginu z funkcją kernelową
      var builder = Kernel.CreateBuilder();
      builder.AddChatCompletion(Session, ServiceAiSymbol);
      builder.Services.AddLogging(loggingBuilder =>
         loggingBuilder.AddDebug()
            .SetMinimumLevel(LogLevel.Trace));
      builder.Plugins.AddFromType<PracownicyPlugin>();
      var kernel = builder.Build(Session);
      
      // Użycie modelu z ustawieniem automatycznego doboru funkcji
      var answer = await kernel.InvokePromptAsync(
         prompt,
         new KernelArguments(
            new OpenAIPromptExecutionSettings
            {
               FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            }
         )
      );
      
      answer.ToTestOutput();
   } 
}