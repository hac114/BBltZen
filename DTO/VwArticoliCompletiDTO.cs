using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class VwArticoliCompletiDTO
    {
        public int ArticoloId { get; set; }

        [Required]
        [StringLength(10)]
        public string TipoArticolo { get; set; } = null!;

        public DateTime? DataCreazione { get; set; }

        public DateTime? DataAggiornamento { get; set; }

        [StringLength(200)]
        public string? NomeArticolo { get; set; }

        [Range(0, 10000)]
        public decimal? PrezzoBase { get; set; }

        [Range(0, 100)]
        public decimal? AliquotaIva { get; set; }

        [Range(0, 1)]
        public int? Disponibile { get; set; }

        [Required]
        [StringLength(100)]
        public string Categoria { get; set; } = null!;
    }
}