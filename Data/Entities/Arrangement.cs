using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data.Entities;
using Stemmesystem.Data.Models;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.MinSpeiding;

namespace Stemmesystem.Server.Data.Entities;

public class Arrangement
{
    public int Id { get; internal set; }

    public Votering? FinnVotering(int voteringId)
    {
        return Saker.SelectMany(s => s.Voteringer).FirstOrDefault(v => v.Id == voteringId);
    }

    public string Navn { get; init; }

    public string? Beskrivelse { get; set; }
    [Column(TypeName="date")]
    public DateTime? Startdato { get; set; }
    [Column(TypeName="date")]
    public DateTime? Sluttdato { get; set; }
    public bool Aktiv { get; set; }


    public IList<Sak> Saker { get; set; } = new List<Sak>();

    public IList<Delegat> Delegater { get; set; } = new List<Delegat>();
        
    public MinSpeidingOptions? MinSpeidingOptions { get; set; }
        

    public Arrangement(string navn)
    {
        Navn = navn;
        Aktiv = true;
    }

    public void LeggTil(Sak sak)
    {
        Saker.Add(sak);
    }

    public void NyDelegat(int nummer, string navn, string? kode = null)
    {
        var delegat = new Delegat(nummer, navn, kode);
        Delegater.Add(delegat);
    }

    public IEnumerable<Votering> AktiveVoteringer()
    {
        return Saker.SelectMany(s => s.Voteringer).Where(v => v.Aktiv);
    }

    public Delegat NyDelegat(NyDelegatModel model, string delegatkode)
    {
        var delegat = new Delegat(model.Nummer, model.Navn, delegatkode)
        {
            Epost = model.Epost,
            Telefon = model.Telefon
        };
        Delegater.Add(delegat);
        return delegat;
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