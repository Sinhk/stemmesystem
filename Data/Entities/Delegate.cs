using System.ComponentModel.DataAnnotations.Schema;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.MinSpeiding;
using Stemmesystem.Shared.Tools;

namespace Stemmesystem.Data.Entities
{
    [Table("Delegat")]
    public class Delegate
    {
        private Arrangement? _arrangement;
        private List<Ballot> _votedIn = new();

        public int Id { get; private set; }
        [Column("Delegatkode")]
        public string DelegateCode { get; set; }
        [Column("Delegatnummer")]
        public int DelegateNumber { get; set; }
        [Column("Navn")]
        public string? Name { get; set; }
        [Column("Gruppe")]
        public string? Group { get; set; }
        [Column("Epost")]
        public string? Email { get; set; }
        [Column("Telefon")]
        public string? Phone { get; set; }

        public int ArrangementId { get; set; }
        public Arrangement Arrangement { get => _arrangement ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Arrangement)); set => _arrangement = value; }

        public IEnumerable<Ballot> VotedIn => _votedIn;
        public DateTime? SmsSentAt => SmsSentAtInternal?.UtcDateTime;
        [Column("SendtSms")]
        public DateTimeOffset? SmsSentAtInternal { get; set; }
        public DateTime? EmailSentAt => EmailSentAtInternal?.UtcDateTime;
        [Column("SendtEmail")]
        public DateTimeOffset? EmailSentAtInternal { get; set; }
        
        [Column("TilStede")]
        public bool Present { get; set; }
        
        public int? MemberId { get; set; }

        internal Delegate(int delegateNumber, string? name, string? delegateCode = null)
        {
            DelegateNumber = delegateNumber;
            Name = name;

            delegateCode ??= RngKeyGenerator.GenerateKey(4);
            DelegateCode = delegateCode;
        }
    }
    
    public static class DelegateMapper
    {
        public static IReadOnlyCollection<Delegate> ToDelegates(this IReadOnlyCollection<Participant> participants,
            bool importCheckIn)
        {
            return participants.Select(p => p.ToDelegate(importCheckIn)).ToList();
        }
    
        public static Delegate ToDelegate(this Participant participant, bool importCheckIn) =>
            new(0, participant.FullName)
            {
                Email = participant.PrimaryEmail,
                Phone = participant.ContactInfo.GetValueOrDefault("Mobiltelefon"),
                Group = participant.GroupName,
                MemberId = participant.MemberNo,
                Present = importCheckIn && participant.CheckedIn.GetValueOrDefault(),
            };
    }
}
