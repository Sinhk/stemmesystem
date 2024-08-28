using System.ComponentModel.DataAnnotations.Schema;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.Tools;

namespace Stemmesystem.Data.Entities
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
        public DateTime? SendtSms => SendtSmsInternal?.UtcDateTime;
        [Column(nameof(SendtSms))]
        public DateTimeOffset? SendtSmsInternal { get; set; }
        public DateTime? SendtEmail => SendtEmailInternal?.UtcDateTime;
        [Column(nameof(SendtEmail))]
        public DateTimeOffset? SendtEmailInternal { get; set; }

        internal Delegat(int delegatnummer, string? navn, string? delegatkode = null)
        {
            Delegatnummer = delegatnummer;
            Navn = navn;

            delegatkode ??= RngKeyGenerator.GenerateKey(4);
            Delegatkode = delegatkode;
        }
    }
}
