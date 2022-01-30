using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Stemmesystem.Server.Data.Entities
{
    [Owned]
    public class Valg
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; internal set; }
        public string Navn { get; init; }
        public int? SortId { get; set; }

        public Valg(string navn, int? sortId = null)
        {
            Navn = navn;
            SortId = sortId;
        }
    }
}
