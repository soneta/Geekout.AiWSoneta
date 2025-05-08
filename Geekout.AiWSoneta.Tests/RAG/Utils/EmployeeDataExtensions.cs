using System.Collections.Generic;
using System.Linq;
using Soneta.Kadry;

namespace Geekout.AiWSoneta.Tests.RAG.Utils;

public static class EmployeeDataExtensions
{
    public static EmployeeData ToEmployeeData(this Pracownik pracownik) =>
        new()
        {
            Id = pracownik.ID.ToString(),
            Nazwisko = pracownik.Nazwisko,
            Imie = pracownik.Imie,
            Kod = pracownik.Kod,
            Doswiadczenie = pracownik.Features["Doswiadczenie"].ToString()
        };
    
    public static IEnumerable<EmployeeData> ToEmployeeData(this IEnumerable<Pracownik> pracownicy) =>
        pracownicy
            .Where(p => p.Features["Doswiadczenie"] is not "" )
            .Select(p => p.ToEmployeeData());
}