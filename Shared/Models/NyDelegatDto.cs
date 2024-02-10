namespace Stemmesystem.Core.Models;

public record NyDelegatDto(int Nummer)
{
    public string? Navn { get; init; }
    public string? Telefon { get; init; }
    public string? Epost { get; init; }
}