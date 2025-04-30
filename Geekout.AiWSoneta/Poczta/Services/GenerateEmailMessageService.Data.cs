using Soneta.Handel;

namespace Geekout.AiWSoneta.Poczta.Services;

public partial class GenerateEmailMessageService
{
    public record SumaPoKorekcie(decimal VAT, decimal Brutto, decimal Netto);

    public record SumaPrzedKorekta(decimal VAT, decimal Brutto, decimal Netto);

    public record OrderInfo(
        string NumerZamowienia,
        StanDokumentuHandlowego Stan,
        PotwierdzenieDokumentuHandlowego Potwierdzenie,
        SumaPrzedKorekta SumaPrzedKorekta,
        SumaPoKorekcie SumaPoKorekcie)
    {
        internal OrderInfo(DokumentHandlowy dokumentHandlowy)
            : this(dokumentHandlowy.NumerPelnyZapisany,dokumentHandlowy.Stan, dokumentHandlowy.Potwierdzenie,
                new SumaPrzedKorekta(dokumentHandlowy.SumaPrzedKorektą.VAT,
                    dokumentHandlowy.SumaPrzedKorektą.Brutto, dokumentHandlowy.SumaPrzedKorektą.Netto),
                new SumaPoKorekcie(dokumentHandlowy.SumaPoKorekcie.VAT,
                    dokumentHandlowy.SumaPoKorekcie.Brutto, dokumentHandlowy.SumaPoKorekcie.Netto)) { }
    }
}
