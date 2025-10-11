using System;

namespace DTO
{
    public class PersonalizzazioneDTO
    {
        public int PersonalizzazioneId { get; set; }
        public string Nome { get; set; } = null!;
        public string Descrizione { get; set; } = null!;
        public DateTime DtCreazione { get; set; }
        public DateTime DtUpdate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
