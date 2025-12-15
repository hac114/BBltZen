using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Comparers
{
    public class StatoPagamentoComparer : IComparer<string>
    {
        // ✅ Definisci l'ordine esatto basato sui nomi degli stati
        private static readonly Dictionary<string, int> _ordinePersonalizzato = new()
        {
            ["non richiesto"] = 1,
            ["pendente"] = 2,
            ["completato"] = 3,
            ["fallito"] = 4,
            ["rimborsato"] = 5
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
