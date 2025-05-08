using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Geekout.AiWSoneta.Tests.SemanticKernel.Utils;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;
using Soneta.Core;
using Soneta.Kadry;

namespace Geekout.AiWSoneta.Tests.RAG.Utils;

[Experimental("SKEXP0010")]
public abstract class RagTestBase : SemanticKernelTestBase
{
   private const string EmbeddingModel = "text-embedding-3-large";
   protected IVectorStoreRecordCollection<string,EmployeeData> VectorStoreCollection { get; private set; }
   protected ITextEmbeddingGenerationService EmbeddingService { get; private set; }
   
   public override void ClassSetup()
   {
      base.ClassSetup();
      InConfigTransaction(() =>
      {
         ImportBusinessXml("doswiadczenie.xml");
      });
      SaveDisposeConfig();
      
      InitializeVectorStoreWithEmployeeData().GetAwaiter().GetResult();
   }

   private async Task InitializeVectorStoreWithEmployeeData()
   {
      // Utworzenie pamięci wektorowej
      var store = new InMemoryVectorStore();
      VectorStoreCollection = store.GetCollection<string, EmployeeData>("skemployee");
      await VectorStoreCollection.CreateCollectionIfNotExistsAsync();
      
      // Pobranie danych pracowników
      var employees = Session.GetKadry().Pracownicy.WgKodu.ToEmployeeData().ToArray();
      // Utworzenie usługi wektoryzacji
      var aiService = Session.GetCore().SystemyZewn.WgSymbol[ServiceAiSymbol];
      EmbeddingService = new AzureOpenAITextEmbeddingGenerationService(
         EmbeddingModel,
         aiService.Config.Serwer,
         aiService.Config.KluczApi);
      
      // Generowanie embeddings dla doświadczenia każdego pracownika
      await Task.WhenAll(employees.Select(async entry =>
         entry.DefinitionEmbedding = await EmbeddingService.GenerateEmbeddingAsync(entry.Doswiadczenie)));

      // Dodanie pracowników do kolekcji
      foreach (var entry in employees)
         await VectorStoreCollection.UpsertAsync(entry);
   }
}