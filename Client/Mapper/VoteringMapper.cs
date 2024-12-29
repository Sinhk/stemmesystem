using Stemmesystem.Shared.Models;

namespace Stemmesystem.Client;

public static class VoteringMapper
{
    public static VoteringInputModel ToInputModel(this VoteringDto dto)
    {
        return new VoteringInputModel
        {
            Id = dto.Id,
            SakId = dto.SakId,
            Tittel = dto.Tittel,
            Beskrivelse = dto.Beskrivelse,
            KanVelge = dto.KanVelge,
            Hemmelig = dto.Hemmelig,
            Valg = dto.Valg.Select(v => new ValgDto
            {
                Id = v.Id,
                Navn = v.Navn
            }).ToList()
        };
    }
}