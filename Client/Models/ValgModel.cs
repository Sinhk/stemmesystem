namespace Stemmesystem.Client.Models;

public record ValgModel()
{
    public Guid Id { get; set; }
    public string? Navn { get; set; }
}