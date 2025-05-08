using Soneta.Core;
using Soneta.Test;
using Soneta.Test.Helpers;

namespace Geekout.AiWSoneta.Tests.SemanticKernel.Utils;

public abstract class SemanticKernelTestBase : TestBase
{
    protected const string ServiceAiSymbol = "Test AI Service";
    
    public override void ClassSetup()
    {
        base.ClassSetup();
        InConfigTransaction(() =>
        {
            var serviceAi = AddConfig(new SystemZewnSerwisAI());
            serviceAi.Symbol = ServiceAiSymbol;
            serviceAi.Config.Serwer = AIConfiguration.AzureOpenAI.Endpoint;
            serviceAi.Config.KluczApi = AIConfiguration.AzureOpenAI.ApiKey;
            serviceAi.Config.NrKlienta = AIConfiguration.AzureOpenAI.DeploymentName;
        });
        SaveDisposeConfig();
    }
}