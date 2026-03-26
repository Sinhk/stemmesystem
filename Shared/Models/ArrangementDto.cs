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
        public string Name { get; init; }
        [ProtoMember(3)]
        public string? Description { get; set; }
        
        [ProtoMember(4)]
        public DateTime? StartDate { get; set; }
        [ProtoMember(5)]
        public DateTime? EndDate { get; set; }

        [ProtoMember(6)] public List<DelegateDto> Delegates { get; init; } = new();
        [ProtoMember(7)] public List<CaseDto> Cases { get; init; } = new();
    }
    
    [ProtoContract]
    public record ArrangementInfo
    {
        [ProtoMember(1)]
        public int Id { get; init; }
        [ProtoMember(2)]
        public string Name { get; init; } = null!;
        [ProtoMember(3)]
        public string? Description { get; set; }
        [ProtoMember(4)]
        public int DelegatesCount { get; set; }
        [ProtoMember(5)]
        public int CasesCount { get; set; }
        [ProtoMember(6)]
        public int BallotsCount { get; set; }
    }

    [ProtoContract]
    public class ArrangementInputModel
    {
        [ProtoMember(1)]
        public int? Id { get; init; }
        [Required(ErrorMessage = "Navn er påkrevd")]
        [ProtoMember(2)]
        public string? Name { get; set; }
        [ProtoMember(3)]
        public string? Description { get; set; }
        [ProtoMember(4)]
        public DateTime? StartDate { get; set; }
        [ProtoMember(5)]
        public DateTime? EndDate { get; set; }

    }
}