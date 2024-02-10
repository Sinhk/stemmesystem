using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Stemmesystem.Core.Models
{
    [ProtoContract]
    public record SakDto
    {
        [ProtoMember(1)]
        public int Id { get; internal init; }
        [ProtoMember(2)]
        public string? Nummer { get; init; }
        [ProtoMember(3, IsRequired = true), Required]
        public string Tittel { get; init; } = null!;

        [ProtoMember(4)]
        public string? Beskrivelse { get; set; }

        [ProtoMember(5, IsRequired = true)] 
        public List<VoteringDto> Voteringer { get; internal init; } = new();
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

        [ProtoMember(5, IsRequired = true)] public List<AdminVoteringDto> Voteringer { get; init; } = new();
        [ProtoMember(6)]
        public int ArrangementId { get; set; }
    }
    
    [ProtoContract]
    public record SakResultatDto
    {
        [ProtoMember(1)]
        public int Id { get; internal init; }
        [ProtoMember(2)]
        public string? Nummer { get; init; }
        [ProtoMember(3)]
        public string? Tittel { get; init; }
        [ProtoMember(4)]
        public string? Beskrivelse { get; set; }

        [ProtoMember(5, IsRequired = true)] public List<VoteringDto> Voteringer { get; internal init; } = new();
        [ProtoMember(6)]
        public int ArrangementId { get; set; }
    }

    [ProtoContract]
    public class SakInputModel
    {
        [ProtoMember(10)]
        public int ArrangementId { get; set; }
        
        [ProtoMember(1)]
        public int Id { get; init; }
        
        [ProtoMember(2)]
        [Required(ErrorMessage = "Saknummer er påkrevd")]
        public string? Nummer { get; set; }

        [ProtoMember(3)]
        [Required(ErrorMessage = "Tittel er påkrevd")]
        public string? Tittel { get; set; }

        [ProtoMember(4)]
        public string? Beskrivelse { get; set; }
        
        [ProtoMember(5, IsRequired = true)]
        public ICollection<VoteringInputModel> Voteringer { get; set; } = new List<VoteringInputModel>();
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