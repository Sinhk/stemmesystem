using Stemmesystem.Tools;
using System;
using System.Collections.Generic;

namespace Stemmesystem.Data
{
    public class Delegat
    {
        private Arrangement? arrangement;
        private List<Votering> harStemmtI = new List<Votering>();

        public int Id { get; private set; }
        public string Delegatkode { get; set; }
        public int Delegatnummer { get; set; }
        public string? Navn { get; set; }
        public string? Gruppe { get; set; }
        public string? Epost { get; set; }
        public string? Telefon { get; set; }

        public int ArrangementId { get; set; }
        public Arrangement Arrangement { get => arrangement ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Arrangement)); set => arrangement = value; }

        public IEnumerable<Votering> HarStemmtI => harStemmtI;
        public DateTimeOffset? SendtSms { get; set; }
        public DateTimeOffset? SendtEmail { get; set; }

        internal Delegat(int delegatnummer, string? navn, string? delegatkode = null)
        {
            Delegatnummer = delegatnummer;
            Navn = navn;

            delegatkode ??= RngKeyGenerator.GenerateKey(4);
            Delegatkode = delegatkode;
        }
    }
}
