namespace DTO
{
    public class StatisticheCarrelloDTO
    {
        public int Id { get; set; }

        // 📈 METRICHE FONDAMENTALI
        public int TotaleOrdini { get; set; }
        public int TotaleProdottiVenduti { get; set; }
        public decimal FatturatoTotale { get; set; }
        public decimal ValoreMedioOrdine { get; set; }
        public decimal ProdottiPerOrdineMedio { get; set; }

        // 🎯 METRICHE CONVERSIONE
        public decimal TassoConversioneBozza { get; set; }
        public decimal TassoConversioneCarrello { get; set; }
        public int CarrelliAbbandonati { get; set; }

        // 📊 DISTRIBUZIONE PRODOTTI
        public required List<DistribuzioneProdottoDTO> DistribuzionePerTipologia { get; set; }
        public required List<ProdottoTopDTO> ProdottiPiuVenduti { get; set; }

        // ⏰ METRICHE TEMPORALI
        public required string FasciaOrariaPiuAttiva { get; set; }
        public int OrdiniOggi { get; set; }
        public decimal FatturatoOggi { get; set; }

        public DateTime DataRiferimento { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }

    public class DistribuzioneProdottoDTO
    {
        public required string TipoArticolo { get; set; }
        public required string Descrizione { get; set; }
        public int TotaleVendite { get; set; }
        public int QuantitaTotale { get; set; }
        public decimal RicavoTotale { get; set; }
        public decimal PercentualeVendite { get; set; }
    }

    public class ProdottoTopDTO
    {
        public required string TipoArticolo { get; set; }
        public int ArticoloId { get; set; }
        public required string NomeProdotto { get; set; }
        public int QuantitaVenduta { get; set; }
        public decimal RicavoTotale { get; set; }
    }
}