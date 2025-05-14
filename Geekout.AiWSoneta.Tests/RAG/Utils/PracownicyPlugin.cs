using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.SemanticKernel;
using Soneta.Business;
using Soneta.Kadry;
using Soneta.Tools;
using Soneta.Types;

namespace Geekout.AiWSoneta.Tests.RAG.Utils;

public sealed class PracownicyPlugin(Session session)
{
    // zwraca listę kodów pracowników wyznaczonego wydziału
    [KernelFunction]
    [System.ComponentModel.Description("Zwraca listę kodów pracowników")]
    public IEnumerable<string> GetPracownicy(
        [System.ComponentModel.Description(
            "Kod wydziału pracownika, przy braku zwraca pełną listę kodów pracowników")] 
        [CanBeNull] 
        string kodWydzialu)
    {
        var kodPracownikówList = session.GetKadry().Pracownicy.GetPracownicy(
                Pracownicy.Range.Pracownicy,
                FromTo.All, 
                GetWydzial(kodWydzialu), 
                true)
            .Cast<Pracownik>()
            .Select(GetKodPracownika).ToList();
        kodPracownikówList.ToTestOutput($"{nameof(GetPracownicy)}({kodWydzialu})");
        return kodPracownikówList;
    }

    private Wydzial GetWydzial(string kodWydzialu) =>
        kodWydzialu.IsNullOrEmpty()
            ? null
            : session.GetKadry().Wydzialy.WgKodu[kodWydzialu]
              ?? throw new RowNotFoundException($"Nie znaleziono wydziału o kodzie {kodWydzialu}");

    private static string GetKodPracownika(Pracownik pracownik) => pracownik.Kod;
}