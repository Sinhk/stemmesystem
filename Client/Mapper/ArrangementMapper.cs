using Stemmesystem.Shared.Models;

namespace Stemmesystem.Client;

public static class ArrangementMapper
{
    public static ArrangementInputModel ToInputModel(this ArrangementDto dto) =>
        new()
        {
            Id = dto.Id,
            Navn = dto.Navn,
            Beskrivelse = dto.Beskrivelse,
            Startdato = dto.Startdato,
            Sluttdato = dto.Sluttdato
        };
}