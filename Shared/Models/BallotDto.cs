using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Stemmesystem.Shared.Models;

[ProtoContract]
[ProtoInclude(13, typeof(AdminBallotDto))]
public record BallotDto
{
    [ProtoMember(1)]
    public int Id { get; init; }
    [ProtoMember(2)]
    public string Title { get; init; }
    [ProtoMember(3)]
    public string Description { get; init; }
    [ProtoMember(4)]
    public int MaxChoices { get; init; }
    [ProtoMember(5)]
    public DateTime? StartTime { get; init; }
    [ProtoMember(6)]
    public DateTime? EndTime { get; set; }
    [ProtoMember(7)]
    public int CaseId { get; init; }

    [ProtoMember(8)] public List<ChoiceDto> Choices { get; init; } = new();
    [ProtoMember(9)]
    public bool Active { get; set; }
    [ProtoMember(10)]
    public bool Locked { get; init; }
    [ProtoMember(11)]
    public bool Published { get; init; }
    [ProtoMember(12)]
    public bool Secret { get; set; }

    public bool Started => StartTime.GetValueOrDefault() > default(DateTime);
}
[ProtoContract]
public record AdminBallotDto : BallotDto
{
    [ProtoMember(1)] public List<DelegateDto> VotedDelegates { get; init; } = new();
    [ProtoMember(2)] public List<VoteDto> Votes { get; init; } = new();
    
    [ProtoMember(13)] public int? DelegatesPresent { get; init; }
}
    
public record CaseInfoDto(int Id, string Title, string Description);


[ProtoContract]
public record BallotResultDto
{
    [ProtoMember(1)]
    public int Id { get; init; }
    [ProtoMember(2)]
    public string Title { get; init; }
    [ProtoMember(3)] public string Description { get; init; }
    [ProtoMember(4)] public List<VoteDto> Votes { get; init; } = new();
    [ProtoMember(5)] public List<ChoiceDto> Choices { get; init; } = new();
    [ProtoMember(6)] public string CaseName { get; init; }
    [ProtoMember(7)] public string CaseNumber { get; init; }
        
    /// <summary>
    /// Number of delegates who cast a vote
    /// </summary>
    [ProtoMember(8)] public int CastVoteCount { get; init; }

    [ProtoMember(9)] public int? DelegatesPresent { get; init; }
}

[ProtoContract]
public record ChoiceDto
{
    [ProtoMember(1)]
    public Guid Id { get; init; }
    [ProtoMember(2)]
    public string Name { get; set; } = null!;
    [ProtoMember(3)]
    public int? SortId { get; set; }
}
    
[ProtoContract]
public record BallotInputModel
{
    [ProtoMember(1)]
    public int CaseId { get; init; }
    [ProtoMember(2)]
    public int? Id { get; init; }
        
    [ProtoMember(4)]
    public string Title { get; set; }
    [ProtoMember(5)]
    public string? Description { get; set; }
    [ProtoMember(6)]
    public bool Secret { get; set; }
    [ProtoMember(7)]
    [DefaultValue(1)]
    [Required, Range(1,int.MaxValue,ErrorMessage = "\"Kan velge\" må være 1 eller mer") ]
    public int MaxChoices { get; set; } = 1;
        
    [ProtoMember(8)]
    public List<ChoiceDto> Choices { get; set; } = new();

    public bool Started { get; set; }
}