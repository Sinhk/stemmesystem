using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stemmesystem.Web.Models
{
    public class VoteringModel
    {
        public int? Id { get; internal set; }
        [Required]
        public string? Tittel { get; set; }
        public string? Beskrivelse { get; set; }
        public bool Hemmelig { get; set; }
        [Required, Range(1,int.MaxValue,ErrorMessage = "\"Kan velge\" må være mer 1 eller mer") ]
        public int KanVelge { get; set; } =1;

        public List<ValgModel> Valg { get; set; } = new();

        public DateTimeOffset? StartTid { get; private set; }
    }
    
    public class ValgModel
    {
        public Guid Id { get; internal set; }
        public string? Navn { get; set; }
        public int? SortId { get; set; }
    }
}