using Stemmesystem.Shared.Models;

namespace Stemmesystem.Client;

public static class SakMapper
{
    public static SakInputModel ToInputModel(this SakDto dto)
    {
        return new SakInputModel
        {
            Id = dto.Id,
            ArrangementId = dto.ArrangementId,
            Nummer = dto.Nummer,
            Tittel = dto.Tittel,
            Beskrivelse = dto.Beskrivelse,
            Voteringer = dto.Voteringer.Select(v => v.ToInputModel()).ToList()
        };
    }
}