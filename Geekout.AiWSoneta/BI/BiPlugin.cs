using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Soneta.BI;
using Soneta.Core.AI.Extensions;
using Soneta.Types;

namespace Geekout.AiWSoneta.BI;

sealed class BiPlugin
{
    [Description(
        "Zwraca miary pewności dopasowania do każdego z dostępnych typów elementów dashboard'ów na postawie zapytania użytkownika")]
    internal static ConfidenceResult GetItemTypeConfidence(
        [Description("ZAPYTANIE użytkownika")]
        string userRequest,
        IEnumerable<ItemType> itemTypes,
        Kernel kernel)
    {
        var function = kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig
            {
                Template = """
                           Na podstawie zapytania użytkownika
                           '{{$userRequest}}'
                           oraz dostępnych typów elementów dashboard'ów w BI
                           '{{$itemTypes}}'
                           zwróć miary pewności dopasowania do każdego z dostępnych typów elementów dashboard'ów w BI
                           """
            }
        );

        return kernel.InvokeWithJsonDeserializationAsync<ConfidenceResult>(
            function,
            new(
#pragma warning disable SKEXP0010
                new OpenAIPromptExecutionSettings
                {
                    ResponseFormat = typeof(ConfidenceResult)
                }
#pragma warning restore SKEXP0010
            )
            {
                ["userRequest"] = userRequest,
                ["itemTypes"] = JsonSerializer.Serialize(itemTypes)
            }
        ).GetAwaiter().GetResult();
    }

    [Description(
        "Zwraca miary pewności dopasowania do każdego z dostępnych obszarów modeli danych na postawie zapytania użytkownika")]
    internal static ConfidenceResult GetAreasConfidence(
        [Description("ZAPYTANIE użytkownika")]
        string userRequest,
        IEnumerable<Area> availableAreasOfDataModels,
        Kernel kernel)
    {
        var function = kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig
            {
                Template = """
                           Na podstawie zapytania użytkownika
                           '{{$userRequest}}'
                           oraz dostępnych obszarów modeli danych w BI
                           '{{$availableAreasOfDataModels}}'
                           zwróć miary pewności dopasowania do każdego z dostępnych obszarów modeli danych
                           """
            }
        );

        return kernel.InvokeWithJsonDeserializationAsync<ConfidenceResult>(
            function,
            new(
#pragma warning disable SKEXP0010
                new OpenAIPromptExecutionSettings
                {
                    ResponseFormat = typeof(ConfidenceResult)
                }
#pragma warning restore SKEXP0010
            )
            {
                [nameof(userRequest)] = userRequest,
                [nameof(availableAreasOfDataModels)] = JsonSerializer.Serialize(availableAreasOfDataModels)
            }
        ).GetAwaiter().GetResult();
    }

    [Description("Zwraca dostępne obszary modeli danych w BI")]
    internal static IEnumerable<Area> GetAreas() =>
        new[]
            {
                AreaOfDataModels.Financial, AreaOfDataModels.Trade, AreaOfDataModels.CRM,
                AreaOfDataModels.HrAndPayroll
            }
            .Select(x => new Area(
                (int)x,
                x.ToString(),
                CaptionAttribute.EnumToString(x)));

    [Description("Zwraca dostępne typy elementów dashboard'ów w BI")]
    internal static IEnumerable<ItemType> GetItemTypes() =>
        new[]
            {
                DashboardItemType.List, DashboardItemType.Indicator, DashboardItemType.PivotReport
            }
            .Select(x => new ItemType(
                (int)x,
                x.ToString(),
                CaptionAttribute.EnumToString(x)));

    [Description("Obszar modelu danych w BI")]
    [DebuggerDisplay("Id = {Id}, Name = {Name}, Description = {Description}")]
    internal record Area(int Id, string Name, string Description);

    [DebuggerDisplay("Length = {Result.Length}")]
    internal record Areas(Area[] Result);

    internal record Dashboard(ItemType Item, VisualizationType Type, Area Area);
    internal record Dashboards(Dashboard[] Result);

    [DebuggerDisplay("Id = {Id}, Name = {Name}, Description = {Description}")]
    internal record ItemType(int Id, string Name, string Description);

    [DebuggerDisplay("Id = {Id}, Name = {Name}")]
    internal record Item(int Id, string Name);

    [DebuggerDisplay("Id = {Id}, Confidence = {Confidence}")]
    internal record ItemConfidence(int Id, int Confidence);

    [DebuggerDisplay("Length = {Result?.Length ?? 0}")]
    internal record ConfidenceResult(ItemConfidence[] Result)
    {
        public int Length => Result?.Length ?? 0;
    }

    [DebuggerDisplay("Length = {Result?.Length}, TotalTokenUsage={TotalTokenUsage}")]
    internal record ConfidenceUsageResult(ItemConfidence[] Result, int? TotalTokenUsage) : ConfidenceResult(Result);
}
