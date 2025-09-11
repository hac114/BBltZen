using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class IngredientiPersonalizzazioneDTO
    {
        public int IngredientePersId { get; set; }
        public int PersCustomId { get; set; }
        public int IngredienteId { get; set; }
        public DateTime DataCreazione { get; set; }
    }
}
