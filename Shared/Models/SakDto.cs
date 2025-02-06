using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Stemmesystem.Shared.Models
{
    [ProtoContract]
    public record SakDto
    {
        [ProtoMember(1)]
        public int Id { get; init; }
        [ProtoMember(2)]
        public required string Nummer { get; init; }
        [ProtoMember(3)]
        public required string Tittel { get; init; }
        [ProtoMember(4)]
        public string? Beskrivelse { get; set; }

        [ProtoMember(5, IsRequired = true)] 
        public List<VoteringDto> Voteringer { get; init; } = [];
        [ProtoMember(6)]
        public int ArrangementId { get; set; }
    }
    
    [ProtoContract]
    public record AdminSakDto
    {
        [ProtoMember(1)]
        public int Id { get; internal init; }
        [ProtoMember(2)]
        public string? Nummer { get; init; }
        [ProtoMember(3)]
        public string? Tittel { get; init; }
        [ProtoMember(4)]
        public string? Beskrivelse { get; set; }

        [ProtoMember(5, IsRequired = true)] 
        public List<AdminVoteringDto> Voteringer { get; init; } = [];
        [ProtoMember(6)]
        public int ArrangementId { get; set; }
    }
    
    [ProtoContract]
    public record SakResultatDto
    {
        [ProtoMember(1)]
        public int Id { get; internal init; }
        [ProtoMember(2)]
        public required string Nummer { get; init; }
        [ProtoMember(3)]
        public required string Tittel { get; init; }
        [ProtoMember(4)]
        public string? Beskrivelse { get; set; }

        [ProtoMember(5, IsRequired = true)] 
        public List<VoteringDto> Voteringer { get; internal init; } = [];
        [ProtoMember(6)]
        public int ArrangementId { get; set; }
    }

    [ProtoContract]
    public class SakInputModel
    {
        [ProtoMember(10)]
        public int ArrangementId { get; set; }
        
        [ProtoMember(1)]
        public int? Id { get; init; }
        
        [ProtoMember(2)]
        [Required(ErrorMessage = "Saknummer er påkrevd")]
        public string? Nummer { get; set; }

        [ProtoMember(3)]
        [Required(ErrorMessage = "Tittel er påkrevd")]
        public string? Tittel { get; set; }

        [ProtoMember(4)]
        public string? Beskrivelse { get; set; }
        
        [ProtoMember(5, IsRequired = true)]
        public ICollection<VoteringInputModel> Voteringer { get; set; } = [];
    }
}