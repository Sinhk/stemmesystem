using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;

namespace Stemmesystem.Web.Services.CSV
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
                if (args.Row.TryGetField<string>("valg", out var valg))
                    return valg.Split(',').ToList();
                return new List<string>();
            });
        }
    } 

}