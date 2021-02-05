using System.ComponentModel.DataAnnotations;

namespace Stemmesystem.Web.Models
{
    public class DelegatModel
    {
        public int? Id { get; internal set; }
        [Required(ErrorMessage = "Delegatnummer er påkrevd")]
        public int? Delegatnummer { get; set; }

        [Required(ErrorMessage = "Navn er påkrevd")]
        [StringLength(100)]
        public string? Navn { get; set; }
        public string? Gruppe { get; set; }

        [EmailAddress(ErrorMessage = "Ikke gyldig epost")]
        public string? Epost { get; set; }
        [Phone(ErrorMessage = "Ikke gyldig telefonnummer")]
        public string? Telefon { get; set; }
    }
}