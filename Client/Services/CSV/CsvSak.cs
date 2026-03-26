using System.Globalization;
using CsvHelper.Configuration;

namespace Stemmesystem.Client.Services.CSV
{
    public record CsvCase
    {
        public string? Number { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Ballot { get; set; }
        public string? BallotDescription { get; set; }
        public bool? SecretBallot { get; set; }
        public List<string>? Choices { get; set; }
        public int? MaxChoices { get; set; }
    }
    
    public sealed class CsvCaseMap : ClassMap<CsvCase>
    {
        public CsvCaseMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(s => s.Choices).Convert(args =>
            {
                if (args.Row.TryGetField<string>("valg", out var valg))
                    return valg.Split(',').ToList();
                return new List<string>();
            });
        }
    } 

}