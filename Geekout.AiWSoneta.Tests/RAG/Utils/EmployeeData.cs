using System;
using Microsoft.Extensions.VectorData;

namespace Geekout.AiWSoneta.Tests.RAG.Utils;

public sealed class EmployeeData
{
        [VectorStoreRecordKey] public string Id { get; set; }
        [VectorStoreRecordData(IsFilterable = true)]
        public string Nazwisko { get; set; }
        [VectorStoreRecordData(IsFilterable = true)]
        public string Imie { get; set; }
        [VectorStoreRecordData] public string Kod { get; set; }
        [VectorStoreRecordData] public string Doswiadczenie { get; set; }
        [VectorStoreRecordVector(Dimensions: 3072)]
        public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
        public override string ToString() => $"{Id} - {Kod} - {Imie} {Nazwisko} - {Doswiadczenie.Substring(0, Math.Min(50, Doswiadczenie.Length))}...";
}