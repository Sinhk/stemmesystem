using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Stemmesystem.Shared.Models
{
    [ProtoContract]
    public record CaseDto
    {
        [ProtoMember(1)]
        public int Id { get; internal init; }
        [ProtoMember(2)]
        public string Number { get; init; }
        [ProtoMember(3)]
        public string Title { get; init; }
        [ProtoMember(4)]
        public string? Description { get; set; }

        [ProtoMember(5)] public List<BallotDto> Ballots { get; internal init; } = new();
        [ProtoMember(6)]
        public int ArrangementId { get; set; }
    }
    
    [ProtoContract]
    public record AdminCaseDto
    {
        [ProtoMember(1)]
        public int Id { get; internal init; }
        [ProtoMember(2)]
        public string? Number { get; init; }
        [ProtoMember(3)]
        public string? Title { get; init; }
        [ProtoMember(4)]
        public string? Description { get; set; }

        [ProtoMember(5)] public List<AdminBallotDto> Ballots { get; init; } = new();
        [ProtoMember(6)]
        public int ArrangementId { get; set; }
    }
    
    [ProtoContract]
    public record CaseResultDto
    {
        [ProtoMember(1)]
        public int Id { get; internal init; }
        [ProtoMember(2)]
        public string Number { get; init; }
        [ProtoMember(3)]
        public string Title { get; init; }
        [ProtoMember(4)]
        public string? Description { get; set; }

        [ProtoMember(5)] public List<BallotDto> Ballots { get; internal init; } = new();
        [ProtoMember(6)]
        public int ArrangementId { get; set; }
    }

    [ProtoContract]
    public class CaseInputModel
    {
        [ProtoMember(10)]
        public int ArrangementId { get; set; }
        
        [ProtoMember(1)]
        public int Id { get; init; }
        
        [ProtoMember(2)]
        [Required(ErrorMessage = "Saknummer er påkrevd")]
        public string? Number { get; set; }

        [ProtoMember(3)]
        [Required(ErrorMessage = "Tittel er påkrevd")]
        public string? Title { get; set; }

        [ProtoMember(4)]
        public string? Description { get; set; }
        
        [ProtoMember(5)]
        public ICollection<BallotInputModel> Ballots { get; set; } = new List<BallotInputModel>();
    }
}