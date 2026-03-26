using System.ComponentModel.DataAnnotations.Schema;

namespace Stemmesystem.Data.Entities
{
    [Table("Gruppe")]
    public class Group
    {
        public int Id { get; private set; }
        [Column("Navn")]
        public string Name { get; set; }
        public IList<Delegate> Delegates { get; set; } = new List<Delegate>();

        public Group(string name)
        {
            Name = name;
        }

    }
}
