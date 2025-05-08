using System.Collections.Generic;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using static NUnit.Framework.TestContext;

namespace Geekout.AiWSoneta.Tests.RAG.Utils;

public static class TestContextExtensions
{
    private static readonly string Separator = new('=', 20);

    public static void ToTestOutput(this FunctionResult result)
    {
        Out.WriteLine();
        Out.WriteLine(Separator);
        Out.WriteLine($"Prompt: {result.RenderedPrompt}");
        Out.WriteLine(Separator);
        Out.WriteLine();
        Out.WriteLine(result);
    }
    
    public static void ToTestOutput(this IEnumerable<VectorSearchResult<EmployeeData>> results)
    {
        foreach (var result in results)
        {
            Out.WriteLine();
            Out.WriteLine(Separator);
            Out.WriteLine($"ID: {result.Record.Id}, Kod: {result.Record.Kod}, Imię: {result.Record.Imie}, Nazwisko: {result.Record.Nazwisko}");
            Out.WriteLine($"Doświadczenie: {result.Record.Doswiadczenie}");
            Out.WriteLine($"Podobieństwo (score): {result.Score}");
            Out.WriteLine(Separator);
            
        }
    }
    
    public static void ToTestOutput(this IEnumerable<ChatMessageContent> contents)
    {
        foreach (var content in contents)
        {
            Out.WriteLine();
            Out.WriteLine(Separator);
            Out.WriteLine($"{content.Role}: {content.Content}");
            Out.WriteLine(Separator);
            
        }
    }
    
    public static void ToTestOutput(this IEnumerable<TextContent> contents)
    {
        foreach (var content in contents)
        {
            Out.WriteLine();
            Out.WriteLine(Separator);
            Out.WriteLine($"{content.Text}");
            Out.WriteLine(Separator);
            
        }
    }
    public static void ToTestOutput(this IEnumerable<string> contents)
    {
        foreach (var content in contents) 
            Out.WriteLine($"- {content}");
    }
}