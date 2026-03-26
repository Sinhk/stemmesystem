using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data.Entities;
using Stemmesystem.Data.Models;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.MinSpeiding;
using DelegateEntity = Stemmesystem.Data.Entities.Delegate;

namespace Stemmesystem.Server.Data.Entities;

public class Arrangement
{
    public int Id { get; internal set; }

    public Ballot? FindBallot(int ballotId)
    {
        return Cases.SelectMany(s => s.Ballots).FirstOrDefault(v => v.Id == ballotId);
    }

    [Column("Navn")]
    public string Name { get; init; }

    [Column("Beskrivelse")]
    public string? Description { get; set; }
    [Column("Startdato", TypeName="date")]
    public DateTime? StartDate { get; set; }
    [Column("Sluttdato", TypeName="date")]
    public DateTime? EndDate { get; set; }
    [Column("Aktiv")]
    public bool Active { get; set; }


    public IList<Case> Cases { get; set; } = new List<Case>();

    public IList<DelegateEntity> Delegates { get; set; } = new List<DelegateEntity>();
        
    public MinSpeidingOptions? MinSpeidingOptions { get; set; }
        

    public Arrangement(string name)
    {
        Name = name;
        Active = true;
    }

    public void Add(Case caseItem)
    {
        Cases.Add(caseItem);
    }

    public DelegateEntity AddDelegate(int number, string name, string? code = null)
    {
        var delegateEntity = new DelegateEntity(number, name, code);
        Delegates.Add(delegateEntity);
        return delegateEntity;
    }

    public IEnumerable<Ballot> ActiveBallots()
    {
        return Cases.SelectMany(s => s.Ballots).Where(v => v.Active);
    }

    public DelegateEntity AddDelegate(NewDelegateModel model, string delegateCode)
    {
        var delegateEntity = new DelegateEntity(model.Number, model.Name, delegateCode)
        {
            Email = model.Email,
            Phone = model.Phone
        };
        Delegates.Add(delegateEntity);
        return delegateEntity;
    }
}

[Owned]
public record MinSpeidingOptions
{
    public static MinSpeidingOptions Default => new() { ImportCheckIn = false };
    public int MinSpeidingId { get; set; }
    [MaxLength(200)]
    public string? MembersApiKey { get; set; }
    public bool ImportCheckIn { get; set; }
    public ParticipantFilter? Filter { get; set; }
}

public static class MinSpeidingOptionsMapper
{
    public static MinSpeidingOptionsDto ToDto(this MinSpeidingOptions options) 
        => new(options.MinSpeidingId, options.MembersApiKey != null, options.ImportCheckIn, options.Filter?.RawFilter);
}