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
        public bool? HemmeligVotering { get; set; }
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
                List<string>? list = null;
                if (args.Row.TryGetField<string>("valg", out var valg)) 
                    list = valg?.Split(',').ToList();

                return list ?? new List<string>();
            });
        }
    } 

}