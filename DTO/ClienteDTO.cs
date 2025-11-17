using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class ClienteDTO
    {
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "L'ID tavolo è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID tavolo deve essere maggiore di 0")]
        public int TavoloId { get; set; }

        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}