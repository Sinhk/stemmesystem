using System.ComponentModel.DataAnnotations.Schema;
using DelegateEntity = Stemmesystem.Data.Entities.Delegate;

namespace Stemmesystem.Data.Entities;

[Table("Stemme")]
public record Vote
{
    public Guid Id { get; private set; }
    [Column("ValgId")]
    public Guid ChoiceId { get; private set; }

    private Vote() { }
    public Vote(Guid choiceId)
    {
        ChoiceId = choiceId;
    }

    public DelegateEntity? Delegate { get; internal set; }
    [Column("DelegatId")]
    public int? DelegateId { get; internal set; }

    [Column("StemmeHash")]
    public string? VoteHash { get; set; }
        
}