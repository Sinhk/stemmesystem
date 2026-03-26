using Stemmesystem.Data.Entities;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Server;

internal static class Mappings
{
    // --- Valg ---
    public static ValgDto ToDto(this Valg v) =>
        new() { Id = v.Id, Navn = v.Navn, SortId = v.SortId };

    // --- Stemme ---
    public static StemmeDto ToDto(this Stemme s) =>
        new(s.Id, s.ValgId, s.DelegatId);

    // --- Delegat ---
    public static DelegatDto ToDto(this Delegat d) => new()
    {
        Id = d.Id,
        Delegatnummer = d.Delegatnummer,
        Navn = d.Navn,
        Gruppe = d.Gruppe,
        Epost = d.Epost,
        Telefon = d.Telefon,
        ArrangementId = d.ArrangementId
    };

    public static AdminDelegatDto ToAdminDto(this Delegat d) => new()
    {
        Id = d.Id,
        Delegatnummer = d.Delegatnummer,
        Navn = d.Navn,
        Gruppe = d.Gruppe,
        Epost = d.Epost,
        Telefon = d.Telefon,
        ArrangementId = d.ArrangementId,
        Delegatkode = d.Delegatkode,
        SendtEmail = d.SendtEmail,
        SendtSms = d.SendtSms,
        TilStede = d.TilStede
    };

    // --- Votering ---
    public static VoteringDto ToDto(this Votering v) => new()
    {
        Id = v.Id,
        Tittel = v.Tittel,
        Beskrivelse = v.Beskrivelse,
        KanVelge = v.KanVelge,
        StartTid = v.StartTid,
        SluttTid = v.SluttTid,
        SakId = v.SakId,
        Valg = v.Valg.Select(x => x.ToDto()).ToList(),
        Aktiv = v.Aktiv,
        Lukket = v.Lukket,
        Publisert = v.Publisert,
        Hemmelig = v.Hemmelig
    };

    public static AdminVoteringDto ToAdminDto(this Votering v) => new()
    {
        Id = v.Id,
        Tittel = v.Tittel,
        Beskrivelse = v.Beskrivelse,
        KanVelge = v.KanVelge,
        StartTid = v.StartTid,
        SluttTid = v.SluttTid,
        SakId = v.SakId,
        Valg = v.Valg.Select(x => x.ToDto()).ToList(),
        Aktiv = v.Aktiv,
        Lukket = v.Lukket,
        Publisert = v.Publisert,
        Hemmelig = v.Hemmelig,
        AvgitStemme = v.AvgitStemme.Select(d => d.ToDto()).ToList(),
        Stemmer = v.Stemmer.Select(s => s.ToDto()).ToList(),
        DelegaterTilstede = v.DelegaterTilstede
    };

    public static VoteringResultatDto ToResultatDto(this Votering v) => new()
    {
        Id = v.Id,
        Tittel = v.Tittel,
        Beskrivelse = v.Beskrivelse ?? "",
        Stemmer = v.Stemmer.Select(s => s.ToDto()).ToList(),
        Valg = v.Valg.Select(x => x.ToDto()).ToList(),
        SakNavn = v.Sak.Tittel,
        SakNummer = v.Sak.Nummer,
        AvgitteStemmer = v.AvgitStemme.Count,
        DelegaterTilstede = v.DelegaterTilstede
    };

    public static VoteringInputModel ToInputModel(this Votering v) => new()
    {
        Id = v.Id,
        SakId = v.SakId,
        Tittel = v.Tittel,
        Beskrivelse = v.Beskrivelse,
        Hemmelig = v.Hemmelig,
        KanVelge = v.KanVelge,
        Valg = v.Valg.Select(x => x.ToDto()).ToList()
    };

    // --- Sak ---
    public static SakDto ToDto(this Sak s) => new()
    {
        Id = s.Id,
        Nummer = s.Nummer,
        Tittel = s.Tittel,
        Beskrivelse = s.Beskrivelse,
        Voteringer = s.Voteringer.Select(v => v.ToDto()).ToList(),
        ArrangementId = s.ArrangementId
    };

    public static AdminSakDto ToAdminDto(this Sak s) => new()
    {
        Id = s.Id,
        Nummer = s.Nummer,
        Tittel = s.Tittel,
        Beskrivelse = s.Beskrivelse,
        Voteringer = s.Voteringer.Select(v => v.ToAdminDto()).ToList(),
        ArrangementId = s.ArrangementId
    };

    public static SakInfoDto ToInfoDto(this Sak s) =>
        new(s.Id, s.Tittel, s.Beskrivelse ?? "");

    // --- Arrangement ---
    public static ArrangementDto ToDto(this Arrangement a) => new()
    {
        Id = a.Id,
        Navn = a.Navn,
        Beskrivelse = a.Beskrivelse,
        Startdato = a.Startdato,
        Sluttdato = a.Sluttdato,
        Delegater = a.Delegater.Select(d => d.ToDto()).ToList(),
        Saker = a.Saker.Select(s => s.ToDto()).ToList()
    };

    public static ArrangementInfo ToInfo(this Arrangement a) => new()
    {
        Id = a.Id,
        Navn = a.Navn,
        Beskrivelse = a.Beskrivelse,
        DelegaterCount = a.Delegater.Count,
        SakerCount = a.Saker.Count,
        VoteringerCount = a.Saker.Sum(s => s.Voteringer.Count)
    };
}
