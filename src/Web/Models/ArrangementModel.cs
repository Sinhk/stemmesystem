using System;
using System.ComponentModel.DataAnnotations;

namespace Stemmesystem.Web.Models
{
    public class ArrangementModel
    {
        public int Id { get; internal set; }
        [Required(ErrorMessage = "Navn er påkrevd")]
        [StringLength(20)]
        public string? Navn { get; set; }
        public string? Beskrivelse { get; set; }
        
        public DateTime? Startdato { get; set; }
        public DateTime? Sluttdato { get; set; }
    }

    public class DelegatModel
    {
        public int Id { get; internal set; }
        [Required(ErrorMessage = "Delegatnummer er påkrevd")]
        public int? Delegatnummer { get; set; }

        [Required(ErrorMessage = "Navn er påkrevd")]
        [StringLength(20)]
        public string? Navn { get; set; }

        [EmailAddress(ErrorMessage = "Ikke gyldig epost")]
        public string? Epost { get; set; }
        [Phone(ErrorMessage = "Ikke gyldig telefonnummer")]
        public string? Telefon { get; set; }
    }

    public class SakModel
    {
        public int Id { get; internal set; }

        [Required(ErrorMessage = "Saknummer er påkrevd")]
        public string? Nummer { get; set; }
        [Required(ErrorMessage = "Tittel er påkrevd")]
        public string? Tittel { get; set; }
    }
}