using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Geekout.AiWSoneta.Tests.RAG.Utils;
using Microsoft.SemanticKernel.Embeddings;
using NUnit.Framework;

namespace Geekout.AiWSoneta.Tests.RAG;

/// <summary>
/// Test demonstrujący wyszukiwanie wektorowe w glosariuszu
/// </summary>
[Experimental("SKEXP0001")]
public class VectorSearchTest : RagTestBase
{
    /// <summary>
    /// Test demonstrujący wyszukiwanie wektorowe w bazie w pamięci
    /// </summary>
    [Test]
    [TestCase("Wypisz mi wszystkie osoby które ukończyły jakąkolwiek szkołę wyższą / studia")]
    public async Task Answer(string query)
    {
        // Wektoryzacja zapytania
        var embeddings = await EmbeddingService.GenerateEmbeddingAsync(query);
        
        // Wyszukiwanie wektorowe
        var searchResults = await VectorStoreCollection.VectorizedSearchAsync(
            embeddings,
            new () { Top = 3 });
        var results = searchResults.Results.ToBlockingEnumerable();
        
        // Wyświetlenie wyniku
        results.ToTestOutput();
    }
}