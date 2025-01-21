using Stemmesystem.Shared.Models;

namespace Stemmesystem.Client;

public class SakFormModel
{
    public SakFormModel() { }
    public SakFormModel(SakDto dto)
    {
        ApplyChanges(dto);
    }

    public int Id { get; set; }
    public string? Nummer { get; set; }
    public string Tittel { get; set; } = string.Empty;
    public string? Beskrivelse { get; set; }

    public SakInputModel ToInputModel(int arrangementId) =>
        new()
        {
            Id = Id,
            Nummer = Nummer,
            Tittel = Tittel,
            Beskrivelse = Beskrivelse,
            ArrangementId = arrangementId,
        };

    public void ApplyChanges(SakDto sak)
    {
        Id = sak.Id;
        Nummer = sak.Nummer;
        Tittel = sak.Tittel;
        Beskrivelse = sak.Beskrivelse;
    }
}

public class VoteringFormModel
{
    public int? Id { get; set; }
    public string Tittel { get; set; } = string.Empty;
    public string? Beskrivelse { get; set; }
    public int KanVelge { get; set; }
    public bool Hemmelig { get; set; }
    public List<ValgDto> Valg { get; set; } = new();

    public VoteringFormModel() { }
    public VoteringFormModel(VoteringDto votering)
    {
        ApplyChanges(votering);
    }

    public VoteringInputModel ToInputModel(int sakId)
    {
        return new VoteringInputModel
        {
            Id = Id,
            Tittel = Tittel,
            Beskrivelse = Beskrivelse,
            KanVelge = KanVelge,
            SakId = sakId,
            Valg = Valg.Select(v => new ValgDto
            {
                Id = new Guid(),
                Navn = v.ToString()
            }).ToList()
        };
    }

    public void ApplyChanges(VoteringDto votering)
    {
        Id = votering.Id;
        Tittel = votering.Tittel;
        Beskrivelse = votering.Beskrivelse;
        KanVelge = votering.KanVelge;
        Valg.Clear();
        foreach (var v in votering.Valg.OrderBy(v => v.SortId))
        {
            Valg.Add(v);
        }
    }
}