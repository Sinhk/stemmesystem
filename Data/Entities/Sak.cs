
using System.Globalization;
using Stemmesystem.Server.Data.Entities;

namespace StemmeSystem.Data.Entities
{
    public class Sak
    {
        private Arrangement? _arrangement;
        private readonly List<Votering> _voteringer = [];

        public int Id { get; internal set; }
        public string Nummer { get; set; }
        public string Tittel { get; set; }

        public string? Beskrivelse { get; set; }
        public int ArrangementId { get; internal set; }
        public Arrangement Arrangement { get => _arrangement ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Arrangement)); internal set => _arrangement = value; }

        public IReadOnlyList<Votering> Voteringer
        {
            get => _voteringer.AsReadOnly();
            init => _voteringer = [..value];
        }

        public void LeggTil(params Votering[] voteringer)
        {
            foreach (var votering in voteringer)
            {
                _voteringer.Add(votering);
            }
        }

        public Sak(string nummer, string tittel)
        {
            Nummer = nummer;
            Tittel = tittel;
        }

        public Sak(int nummer, string tittel) : this(nummer.ToString(CultureInfo.InvariantCulture), tittel)
        {
        }
    }
}
