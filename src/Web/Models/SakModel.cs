using System.ComponentModel.DataAnnotations;

namespace Stemmesystem.Web.Models
{
    public class SakModel
    {
        public int Id { get; internal set; }

        [Required(ErrorMessage = "Saknummer er påkrevd")]
        public string? Nummer { get; set; }
        [Required(ErrorMessage = "Tittel er påkrevd")]
        public string? Tittel { get; set; }
    }
}