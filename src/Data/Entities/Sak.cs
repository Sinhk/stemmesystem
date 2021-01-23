using System;
using System.Collections.Generic;

namespace Stemmesystem.Data
{
    public class Sak
    {
        private Arrangement? arrangement;

        public int Id { get; internal set; }
        public string Nummer { get; private set; }
        public string Tittel { get; private set; }

        public Arrangement Arrangement { get => arrangement ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Arrangement)); internal set => arrangement = value; }
        public int ArrangementId { get; internal set; }

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

        public string? Beskrivelse { get; set; }

        public IList<Votering> Voteringer { get; set; } = new List<Votering>();

    }
}
