using System.Collections.Generic;

namespace Stemmesystem.Data
{
    public class Gruppe
    {
        public int Id { get; private set; }
        public string Navn { get; set; }
        public IList<Delegat> Delegater { get; set; } = new List<Delegat>();

        public Gruppe(string navn)
        {
            Navn = navn;
        }

    }
}
