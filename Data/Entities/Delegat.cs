using System.ComponentModel.DataAnnotations.Schema;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.MinSpeiding;
using Stemmesystem.Shared.Tools;

namespace Stemmesystem.Data.Entities
{
    public class Delegat
    {
        private Arrangement? arrangement;
        private List<Votering> harStemmtI = new();

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
        
        public bool TilStede { get; set; }
        
        public int? MemberId { get; set; }

        public Delegat(int delegatnummer, string? navn, string? delegatkode = null)
        {
            Delegatnummer = delegatnummer;
            Navn = navn;

            delegatkode ??= KeyGenerator.GenerateKey(6);
            Delegatkode = delegatkode;
        }
    }
    
    public static class DelegatMapper
    {
        public static IReadOnlyCollection<Delegat> ToDelegates(this IReadOnlyCollection<Participant> participants,
            bool importCheckIn)
        {
            return participants.Select(p => p.ToDelegate(importCheckIn)).ToList();
        }
    
        public static Delegat ToDelegate(this Participant participant, bool importCheckIn) =>
            new(0, participant.FullName)
            {
                 Epost = participant.PrimaryEmail,
                 Telefon = participant.ContactInfo.GetValueOrDefault("Mobiltelefon"),
                 Gruppe = participant.GroupName,
                 MemberId = participant.MemberNo,
                TilStede = importCheckIn && participant.CheckedIn.GetValueOrDefault(),
            };
    }
}
