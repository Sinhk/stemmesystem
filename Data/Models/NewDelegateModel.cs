namespace Stemmesystem.Data.Models
{
    public record NewDelegateModel(int Number)
    {
        public string? Name { get; init; }
        public string? Phone { get; init; }
        public string? Email { get; init; }
    }
}
