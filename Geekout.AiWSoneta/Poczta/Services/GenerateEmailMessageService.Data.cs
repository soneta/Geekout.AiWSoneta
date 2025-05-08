using Soneta.Handel;

namespace Geekout.AiWSoneta.Poczta.Services;

public partial class GenerateEmailMessageService
{
    public record OrderInfo(
        string NumerZamowienia,
        StanDokumentuHandlowego Stan,
        PotwierdzenieDokumentuHandlowego Potwierdzenie,
        decimal WartoscBrutto,
        string TerminDostawy,
        string SposobDostawy, 
        int IloscPozycjiZamowienia)
    {
        internal OrderInfo(DokumentHandlowy dokumentHandlowy)
            : this(dokumentHandlowy.NumerPelnyZapisany, dokumentHandlowy.Stan,
                dokumentHandlowy.Potwierdzenie, dokumentHandlowy.BruttoCy.Value, dokumentHandlowy.Dostawa.Termin.ToString(),
                dokumentHandlowy.Dostawa.Sposob, dokumentHandlowy.Pozycje.Count) { }
    }
}
