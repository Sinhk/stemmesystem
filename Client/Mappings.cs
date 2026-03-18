using Stemmesystem.Client.Services.CSV;
using Stemmesystem.Shared.Models;
using Stemmesystem.Web.Services.CSV;

namespace Stemmesystem.Client;

internal static class Mappings
{
    // --- CsvDelegat → DelegatInputModel ---
    public static DelegatInputModel ToInputModel(this CsvDelegat d) => new()
    {
        Delegatnummer = d.Delegatnummer,
        Navn = d.Navn,
        Gruppe = d.Gruppe,
        Epost = d.Epost,
        Telefon = d.Telefon
        // Id and ArrangementId are left as default
    };

    // --- DTO → InputModel (create new) ---

    public static ArrangementInputModel ToInputModel(this ArrangementDto dto) => new()
    {
        Id = dto.Id,
        Navn = dto.Navn,
        Beskrivelse = dto.Beskrivelse,
        Startdato = dto.Startdato,
        Sluttdato = dto.Sluttdato
    };

    public static DelegatInputModel ToInputModel(this DelegatDto dto) => new()
    {
        Id = dto.Id != default ? dto.Id : null,
        Delegatnummer = dto.Delegatnummer,
        Navn = dto.Navn,
        Gruppe = dto.Gruppe,
        Epost = dto.Epost,
        Telefon = dto.Telefon,
        ArrangementId = dto.ArrangementId
    };

    public static SakInputModel ToInputModel(this SakDto dto) => new()
    {
        Id = dto.Id,
        Nummer = dto.Nummer,
        Tittel = dto.Tittel,
        Beskrivelse = dto.Beskrivelse,
        ArrangementId = dto.ArrangementId,
        Voteringer = dto.Voteringer.Select(v => v.ToInputModel()).ToList()
    };

    public static VoteringInputModel ToInputModel(this VoteringDto dto) => new()
    {
        Id = dto.Id,
        SakId = dto.SakId,
        Tittel = dto.Tittel,
        Beskrivelse = dto.Beskrivelse,
        Hemmelig = dto.Hemmelig,
        KanVelge = dto.KanVelge,
        Valg = dto.Valg.ToList()
    };

    // --- DTO → InputModel (update in place) ---

    public static void ApplyToInputModel(this ArrangementDto dto, ArrangementInputModel model)
    {
        model.Id = dto.Id;
        model.Navn = dto.Navn;
        model.Beskrivelse = dto.Beskrivelse;
        model.Startdato = dto.Startdato;
        model.Sluttdato = dto.Sluttdato;
    }

    public static void ApplyToInputModel(this AdminDelegatDto dto, DelegatInputModel model)
    {
        model.Id = dto.Id != default ? dto.Id : null;
        model.Delegatnummer = dto.Delegatnummer;
        model.Navn = dto.Navn;
        model.Gruppe = dto.Gruppe;
        model.Epost = dto.Epost;
        model.Telefon = dto.Telefon;
        model.ArrangementId = dto.ArrangementId;
    }

    public static void ApplyToInputModel(this SakDto dto, SakInputModel model)
    {
        model.Id = dto.Id;
        model.Nummer = dto.Nummer;
        model.Tittel = dto.Tittel;
        model.Beskrivelse = dto.Beskrivelse;
        model.ArrangementId = dto.ArrangementId;
    }

    public static void ApplyToInputModel(this VoteringDto dto, VoteringInputModel model)
    {
        model.Id = dto.Id;
        model.SakId = dto.SakId;
        model.Tittel = dto.Tittel;
        model.Beskrivelse = dto.Beskrivelse;
        model.Hemmelig = dto.Hemmelig;
        model.KanVelge = dto.KanVelge;
        model.Valg = dto.Valg.ToList();
    }

    // --- Collection helpers ---

    public static ICollection<DelegatInputModel> ToInputModels(this IEnumerable<DelegatDto> delegater) =>
        delegater.Select(d => d.ToInputModel()).ToList();

    public static ICollection<SakInputModel> ToInputModels(this IEnumerable<SakDto> saker) =>
        saker.Select(s => s.ToInputModel()).ToList();

    public static List<VoteringInputModel> ToInputModels(this IEnumerable<VoteringDto> voteringer) =>
        voteringer.Select(v => v.ToInputModel()).ToList();
}
