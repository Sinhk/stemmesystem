using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Stemmesystem.Server.Data.Entities
{
    [Owned]
    public class Choice
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; internal set; }
        [Column("Navn")]
        public string Name { get; init; }
        public int? SortId { get; set; }

        public Choice(string name, int? sortId = null)
        {
            Name = name;
            SortId = sortId;
        }
    }
}
