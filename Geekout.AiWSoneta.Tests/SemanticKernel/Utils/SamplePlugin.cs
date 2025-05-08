using Microsoft.SemanticKernel;
using Soneta.Business;

namespace Geekout.AiWSoneta.Tests.SemanticKernel.Utils;

public class SamplePlugin(Session session)
{
    [KernelFunction] 
    public string GetSampleText() => nameof(GetSampleText) + session.LiveCounter;

}