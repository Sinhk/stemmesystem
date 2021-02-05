using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stemmesystem.Web.Models
{
    public class ArrangementModel
    {
        public int? Id { get; internal set; }
        [Required(ErrorMessage = "Navn er påkrevd")]
        [StringLength(20)]
        public string? Navn { get; set; }
        public string? Beskrivelse { get; set; }
        
        public DateTime? Startdato { get; set; }
        public DateTime? Sluttdato { get; set; }
        public IList<DelegatModel>? Delegater { get; set; }
        public IList<SakModel>? Saker { get; set; }
    }
}