using Riok.Mapperly.Abstractions;
using Stemmesystem.Client.Pages.Admin;
using Stemmesystem.Data.Entities;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Server.Services;

[Mapper]
[UseStaticMapper(typeof(DelegatMapper))]
public static partial class SakMapper
{
    public static IQueryable<SakDto> ToDtos(this IQueryable<Sak> query) => query.Select(s =>
        new SakDto
        {
            Id = s.Id,
            Tittel = s.Tittel,
            Beskrivelse = s.Beskrivelse,
            Voteringer = s.Voteringer.AsQueryable().ToDtos().ToList()
        });
    
    public static SakDto ToDto(this Sak sak) => new()
    {
        Id = sak.Id,
        Tittel = sak.Tittel,
        Beskrivelse = sak.Beskrivelse,
        Voteringer = sak.Voteringer.Select(v => v.ToDto()).ToList()
    };

    
    public static partial IQueryable<AdminSakDto> ToAdminDtos(this IQueryable<Sak> sak);
    
    [MapperIgnoreSource(nameof(Sak.Arrangement))]
    public static partial AdminSakDto ToAdminDto(this Sak sak);

    public static partial IQueryable<VoteringDto> ToDtos(this IQueryable<Votering> query);
    
    [MapperIgnoreSource(nameof(Votering.DelegaterTilstede))]
    [MapperIgnoreSource(nameof(Votering.Stemmer))]
    [MapperIgnoreSource(nameof(Votering.Sak))]
    [MapperIgnoreSource(nameof(Votering.AvgitStemme))]
    [MapperIgnoreTarget(nameof(VoteringDto.Startet))]
    public static partial VoteringDto ToDto(this Votering votering);
    
    [MapperIgnoreSource(nameof(Votering.Sak))]
    [MapperIgnoreTarget(nameof(AdminVoteringDto.Startet))]
    public static partial AdminVoteringDto ToAdminDto(this Votering votering);
    
    [MapperIgnoreSource(nameof(Stemme.Delegat))]
    public static partial StemmeDto ToDto(this Stemme stemme);
    
    public static partial ValgDto ToDto(this Valg stemme);
}