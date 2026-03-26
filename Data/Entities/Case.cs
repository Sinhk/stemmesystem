using Stemmesystem.Server.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stemmesystem.Data.Entities
{
    [Table("Sak")]
    public class Case
    {
        private Arrangement? _arrangement;

        public int Id { get; internal set; }
        [Column("Nummer")]
        public string Number { get; set; }
        [Column("Tittel")]
        public string Title { get; set; }

        [Column("Beskrivelse")]
        public string? Description { get; set; }
        public int ArrangementId { get; internal set; }
        public Arrangement Arrangement { get => _arrangement ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Arrangement)); internal set => _arrangement = value; }
        public IList<Ballot> Ballots { get; set; } = new List<Ballot>();


        public void Add(params Ballot[] ballots)
        {
            foreach (var ballot in ballots)
            {
                Ballots.Add(ballot);
            }
        }

        public Case(string number, string title)
        {
            Number = number;
            Title = title;
        }

        public Case(int number, string title) : this(number.ToString(), title)
        {
        }
    }
}
