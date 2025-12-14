using System.Collections.Generic;

namespace Repository.Comparers
{
    public class StatoOrdineComparer : IComparer<string>
    {
        // ✅ Definisci l'ordine esatto basato sui nomi degli stati
        private static readonly Dictionary<string, int> _ordinePersonalizzato = new()
        {
            ["bozza"] = 1,
            ["in carrello"] = 2,
            ["in coda"] = 3,
            ["in preparazione"] = 4,
            ["pronta consegna"] = 5,
            ["consegnato"] = 6,
            ["sospeso"] = 7,
            ["annullato"] = 8
        };

        public int Compare(string? x, string? y)
        {
            // Ottieni la priorità; se non trovata, usa int.MaxValue (vai in fondo)
            var prioritaX = _ordinePersonalizzato.GetValueOrDefault(x ?? "", int.MaxValue);
            var prioritaY = _ordinePersonalizzato.GetValueOrDefault(y ?? "", int.MaxValue);

            return prioritaX.CompareTo(prioritaY);
        }
    }
}