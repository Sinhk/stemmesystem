using Riok.Mapperly.Abstractions;
using Stemmesystem.Data.Entities;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Server.Services;

[Mapper]
[UseStaticMapper(typeof(DelegatMapper))]
[UseStaticMapper(typeof(SakMapper))]
public static partial class ArrangementMapper
{
    public static IQueryable<ArrangementDto> ToDtos(this IQueryable<Arrangement> query) => query.Select(a =>
        new ArrangementDto
        {
            Id = a.Id,
            Beskrivelse = a.Beskrivelse,
            Navn = a.Navn,
            Sluttdato = a.Sluttdato,
            Startdato = a.Startdato,
            Delegater = a.Delegater.AsQueryable().ToDtos().ToList(),
            Saker = a.Saker.AsQueryable().ToDtos().ToList()
        });

    [MapperIgnoreSource(nameof(Arrangement.MinSpeidingOptions))]
    [MapperIgnoreSource(nameof(Arrangement.Aktiv))]
    public static partial ArrangementDto ToDto(this Arrangement arrangement);   
    
    public static ArrangementInfo ToInfo(this Arrangement a)
    {
        return new ArrangementInfo
        {
            Id = a.Id,
            Navn = a.Navn,
            Beskrivelse = a.Beskrivelse,
            AntallDelegater = a.Delegater.Count,
            AntallSaker = a.Saker.Count
        };
    }
    
    public static IQueryable<ArrangementInfo> ToInfos(this IQueryable<Arrangement> query) => query.Select(a =>  new ArrangementInfo
        {
            Id = a.Id,
            Navn = a.Navn,
            Beskrivelse = a.Beskrivelse,
            AntallDelegater = a.Delegater.Count,
            AntallSaker = a.Saker.Count
        });
}

[Mapper]
public static partial class DelegatMapper
{
    public static IQueryable<DelegatDto> ToDtos(this IQueryable<Delegat> query) => query.Select(d =>
        new DelegatDto
        {
            Id = d.Id,
            Navn = d.Navn,
            Epost = d.Epost,
            Telefon = d.Telefon,
            ArrangementId = d.ArrangementId,
            Delegatnummer = d.Delegatnummer,
            Gruppe = d.Gruppe
        });
    
    public static partial IQueryable<AdminDelegatDto> ToAdminDtos(this IQueryable<Delegat> query);
    
    [MapperIgnoreSource(nameof(Delegat.MemberId))]
    [MapperIgnoreSource(nameof(Delegat.Arrangement))]
    [MapperIgnoreSource(nameof(Delegat.HarStemmtI))]
    [MapperIgnoreSource(nameof(Delegat.SendtEmailInternal))]
    [MapperIgnoreSource(nameof(Delegat.SendtSmsInternal))]
    public static partial AdminDelegatDto ToAdminDto(this Delegat delegat);
    
    
    [MapperIgnoreSource(nameof(Delegat.MemberId))]
    [MapperIgnoreSource(nameof(Delegat.Arrangement))]
    [MapperIgnoreSource(nameof(Delegat.HarStemmtI))]
    [MapperIgnoreSource(nameof(Delegat.SendtEmailInternal))]
    [MapperIgnoreSource(nameof(Delegat.SendtSmsInternal))]
    [MapperIgnoreSource(nameof(Delegat.TilStede))]
    [MapperIgnoreSource(nameof(Delegat.Delegatkode))]
    [MapperIgnoreSource(nameof(Delegat.SendtEmail))]
    [MapperIgnoreSource(nameof(Delegat.SendtSms))]
    public static partial DelegatDto ToDto(this Delegat delegat);
    
    [UserMapping]
    internal static int DelegatToInt(Delegat d) => d.Id;
    
}