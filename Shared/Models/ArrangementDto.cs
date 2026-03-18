using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Stemmesystem.Shared.Models
{
    [ProtoContract]
    public record ArrangementDto
    {
        [ProtoMember(1)]
        public int Id { get; init; }
        [ProtoMember(2)]
        [Required(ErrorMessage = "Navn er påkrevd")]
        [StringLength(20)]
        public string Navn { get; init; }
        [ProtoMember(3)]
        public string? Beskrivelse { get; set; }
        
        [ProtoMember(4)]
        public DateTime? Startdato { get; set; }
        [ProtoMember(5)]
        public DateTime? Sluttdato { get; set; }

        [ProtoMember(6)] public List<DelegatDto> Delegater { get; init; } = new();
        [ProtoMember(7)] public List<SakDto> Saker { get; init; } = new();
    }
    
    [ProtoContract]
    public record ArrangementInfo
    {
        [ProtoMember(1)]
        public int Id { get; init; }
        [ProtoMember(2)]
        public string Navn { get; init; } = null!;
        [ProtoMember(3)]
        public string? Beskrivelse { get; set; }
        [ProtoMember(4)]
        public int DelegaterCount { get; set; }
        [ProtoMember(5)]
        public int SakerCount { get; set; }
        [ProtoMember(6)]
        public int VoteringerCount { get; set; }
    }

    [ProtoContract]
    public class ArrangementInputModel
    {
        [ProtoMember(1)]
        public int? Id { get; set; }
        [Required(ErrorMessage = "Navn er påkrevd")]
        [ProtoMember(2)]
        public string? Navn { get; set; }
        [ProtoMember(3)]
        public string? Beskrivelse { get; set; }
        [ProtoMember(4)]
        public DateTime? Startdato { get; set; }
        [ProtoMember(5)]
        public DateTime? Sluttdato { get; set; }

    }
}