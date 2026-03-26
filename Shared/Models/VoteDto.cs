namespace Stemmesystem.Shared.Models;


public record VoteDto(Guid Id, Guid ChoiceId, int? DelegateId);