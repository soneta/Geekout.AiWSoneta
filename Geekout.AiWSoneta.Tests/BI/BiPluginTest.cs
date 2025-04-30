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
using Soneta.Test.Helpers;
using Geekout.AiWSoneta.BI;

namespace Geekout.AiWSoneta.Tests.BI;

public class BiPluginTest
{
    [Test]
    public void GetAreasConfidenceTest()
    {
        var kernel = GetKernel();
        var areas = BiPlugin.GetAreasConfidence(
            "księgowość i finanse", BiPlugin.GetAreas(),
            kernel);
        areas.Result.Should().HaveCount(4);
        var comparer = new Comparer();
        areas.Result[0].Should().Be(new ((int)AreaOfDataModels.Financial, 90), comparer);
        areas.Result[1].Should().Be(new ((int)AreaOfDataModels.Trade, 50), comparer);
        areas.Result[2].Should().Be(new ((int)AreaOfDataModels.CRM, 20), comparer);
        areas.Result[3].Should().Be(new ((int)AreaOfDataModels.HrAndPayroll, 10), comparer);
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

    private static Kernel GetKernel(
        Action<IKernelBuilder> configureBuilder = null,
        Action<Kernel> configure = null) =>
        SemanticKernelHelper.GetTestKernel(configureBuilder, configure);

    class Comparer : IEqualityComparer<BiPlugin.ItemConfidence>
    {
        public bool Equals(BiPlugin.ItemConfidence x, BiPlugin.ItemConfidence y) =>
            x?.Id == y?.Id && Math.Abs((x?.Confidence ?? 0) - (y?.Confidence ?? 0))<20;

        public int GetHashCode(BiPlugin.ItemConfidence obj) => obj.GetHashCode();
    }


    // [DebuggerDisplay("AreaId = {AreaId}, Confidence = {Confidence}")]
    // internal record AreaConfidence(int AreaId, int Confidence);

    // [DebuggerDisplay("Count = {Result.Length}")]
    // internal record AreasConfidenceResult(AreaConfidence[] Result);

    // [DebuggerDisplay("ItemTypeId = {ItemTypeId}, Confidence = {Confidence}")]
    // internal record ItemTypeConfidence(int ItemTypeId, int Confidence);
    //
    // [DebuggerDisplay("Count = {Result.Length}")]
    // internal record ItemTypeConfidenceResult(ItemTypeConfidence[] Result);
}


