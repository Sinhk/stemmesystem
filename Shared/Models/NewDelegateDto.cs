namespace Stemmesystem.Shared.Models;

public record NewDelegateDto(int Number)
{
    public string? Name { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}