using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class DimensioneBicchiereDTO
    {
        public int DimensioneBicchiereId { get; set; }

        [Required(ErrorMessage = "La sigla è obbligatoria")]
        [StringLength(3, MinimumLength = 1, ErrorMessage = "La sigla deve avere tra 1 e 3 caratteri")]
        [RegularExpression(@"^[A-Z]{1,3}$", ErrorMessage = "La sigla deve contenere solo lettere maiuscole")]
        public required string Sigla { get; set; }

        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La descrizione deve avere tra 2 e 50 caratteri")]
        public required string Descrizione { get; set; }

        [Required(ErrorMessage = "La capienza è obbligatoria")]
        [Range(250, 1000, ErrorMessage = "La capienza deve essere tra 250 e 1000 ml")]
        public decimal Capienza { get; set; }

        [Required(ErrorMessage = "L'unità di misura è obbligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID unità di misura deve essere maggiore di 0")]
        public int UnitaMisuraId { get; set; }        

        [Required(ErrorMessage = "Il prezzo base è obbligatorio")]
        [Range(0.01, 100, ErrorMessage = "Il prezzo base deve essere tra 0.01 e 100")]
        public decimal PrezzoBase { get; set; }

        [Required(ErrorMessage = "Il moltiplicatore è obbligatorio")]
        [Range(0.1, 3.0, ErrorMessage = "Il moltiplicatore deve essere tra 0.1 e 3.0")]
        public decimal Moltiplicatore { get; set; }

        [Required(ErrorMessage = "L'unità di misura è obbligatoria")]
        public UnitaDiMisuraDTO? UnitaMisura { get; set; } = new();
    }
}