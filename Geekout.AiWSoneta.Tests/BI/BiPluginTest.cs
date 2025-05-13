using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using NUnit.Framework;
using Soneta.BI;
using Geekout.AiWSoneta.BI;
using System.Linq;
using Soneta.Core.AI.Extensions;
using Geekout.AiWSoneta.Tests.SemanticKernel.Utils;

namespace Geekout.AiWSoneta.Tests.BI;

public class BiPluginTest : SemanticKernelTestBase
{
    private Kernel GetKernel()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddChatCompletion(Session, ServiceAiSymbol);
        return builder.Build(Session);
    }

    [Test]
    public void GetAreasConfidenceTest()
    {
        var failedCount = 0;
        const int repeats = 10;
        for (var repeat = 0; repeat < repeats; ++repeat)
        {
            var kernel = GetKernel();
            var areas = BiPlugin.GetAreasConfidence(
                "księgowość i finanse", BiPlugin.GetAreas(),
                kernel);
            var result = areas.Result.OrderByDescending(x => x.Confidence).ToArray();
            try
            {
                result.Should().HaveCount(4);
                TestContext.Out.WriteLine(repeat);
                for (var index = 0; index < result.Length; index++)
                {
                    var itemConfidence = result[index];
                    TestContext.Out.WriteLine(
                        $"{index}. {((AreaOfDataModels)itemConfidence.Id).ToString()} - {itemConfidence.Confidence}");
                }

                var comparer = new Comparer();
                result[0].Should().Be(new((int)AreaOfDataModels.Financial, 90), comparer);
                result[1].Should().Be(new((int)AreaOfDataModels.Trade, 50), comparer);
                result[2].Should().Be(new((int)AreaOfDataModels.CRM, 20), comparer);
                result[3].Should().Be(new((int)AreaOfDataModels.HrAndPayroll, 10), comparer);
            }
            catch (AssertionException)
            {
                failedCount++;
            }
        }
        Assert.AreEqual(0, failedCount, $"Passed: {repeats - failedCount}/{repeats}");
    }

    [Test]
    public void GetItemTypeConfidenceTest()
    {
        var kernel = GetKernel();
        var areas = BiPlugin.GetItemTypeConfidence(
            "Indicator i raport", BiPlugin.GetItemTypes(),
            kernel);
        areas.Result.Should().HaveCount(3);
        TestContext.Out.WriteLine(
            JsonSerializer.Serialize(areas,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
        // var comparer = new Comparer();
        // areas.Result[0].Should().Be(new ((int)DashboardItemType.List, 90), comparer);
        // areas.Result[1].Should().Be(new ((int)DashboardItemType.Indicator, 90), comparer);
        // areas.Result[2].Should().Be(new ((int)DashboardItemType.PivotReport, 20), comparer);
    }

    [Test]
    public async Task GetItemTypeConfidenceWithVector()
    {
        var kernel = GetKernel();

#pragma warning disable SKEXP0001
        var embeddingService = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
#pragma warning restore SKEXP0001

        var embeddings = await embeddingService.GenerateEmbeddingsAsync(["Indicator i raport"]);
        //TODO Try calculating confidence from embeddings
    }


    private class Comparer : IEqualityComparer<BiPlugin.ItemConfidence>
    {
        public bool Equals(BiPlugin.ItemConfidence x, BiPlugin.ItemConfidence y) =>
            x?.Id == y?.Id && Math.Abs((x?.Confidence ?? 0) - (y?.Confidence ?? 0))<=25;

        public int GetHashCode(BiPlugin.ItemConfidence obj) => obj.GetHashCode();
    }
}


