namespace Stemmesystem.Server.Data.Entities
{
    public class Sak
    {
        private Arrangement? arrangement;

        public int Id { get; internal set; }
        public string Nummer { get; set; }
        public string Tittel { get; set; }

        public string? Beskrivelse { get; set; }
        public int ArrangementId { get; internal set; }
        public Arrangement Arrangement { get => arrangement ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Arrangement)); internal set => arrangement = value; }
        public IList<Votering> Voteringer { get; set; } = new List<Votering>();


        public void LeggTil(params Votering[] voteringer)
        {
            foreach (var votering in voteringer)
            {
                Voteringer.Add(votering);
            }
        }

        public Sak(string nummer, string tittel)
        {
            Nummer = nummer;
            Tittel = tittel;
        }

        public Sak(int nummer, string tittel) : this(nummer.ToString(), tittel)
        {
        }
    }
}
