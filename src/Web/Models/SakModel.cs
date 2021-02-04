using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stemmesystem.Web.Models
{
    public class SakModel
    {
        public int? Id { get; internal set; }

        [Required(ErrorMessage = "Saknummer er påkrevd")]
        public string? Nummer { get; set; }

        [Required(ErrorMessage = "Tittel er påkrevd")]
        public string? Tittel { get; set; }

        public string? Beskrivelse { get; set; }
        public IList<VoteringModel>? Voteringer { get; internal set; }
    }
    /*
    public class SakModel
    {
        public int Id { get; internal set; }

        public string? Nummer { get; init; }

        public string? Tittel { get; init; }

        public string? Beskrivelse { get; init; }
        public IList<VoteringModel> Voteringer { get; internal set; } = null!;
    }
    */
}