using System.Globalization;
using CsvHelper.Configuration;

namespace Stemmesystem.Client.Services.CSV
{
    public record CsvSak
    {
        public string? Nummer { get; set; }
        public string? Tittel { get; set; }
        public string? Beskrivelse { get; set; }
        public string? Votering { get; set; }
        public string? VoteringBeskrivelse { get; set; }
        public List<string>? Valg { get; set; }
        public int? KanVelge { get; set; }
    }
    
    public sealed class CsvSakMap : ClassMap<CsvSak>
    {
        public CsvSakMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(s => s.Valg).Convert(args =>
            {
                if (args.Row.TryGetField<string>("valg", out var valg) && valg is not null)
                    return valg.Split(',').ToList();
                return [];
            });
        }
    } 

}