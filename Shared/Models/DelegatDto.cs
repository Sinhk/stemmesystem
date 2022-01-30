using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Stemmesystem.Shared.Models
{
    [ProtoContract]
    [ProtoInclude(8,typeof(AdminDelegatDto))]
    public record DelegatDto
    {
        [ProtoMember(1)]
        public int Id { get; internal set; }
        [ProtoMember(2)]
        public int? Delegatnummer { get; set; }
        [ProtoMember(3)]
        public string? Navn { get; set; }
        [ProtoMember(4)]
        public string? Gruppe { get; set; }
        [ProtoMember(5)]
        public string? Epost { get; set; }
        [ProtoMember(6)]
        public string? Telefon { get; set; }
        [ProtoMember(7)]
        public int ArrangementId { get; set; }
    }

    [ProtoContract]
    public record AdminDelegatDto : DelegatDto
    {
        [ProtoMember(9)]
        public string? Delegatkode { get; init; }
        
        [ProtoMember(10)]
        public DateTime? SendtEmail { get; set; }
        [ProtoMember(11)]
        public DateTime? SendtSms { get; set; }
    }
    
    [ProtoContract]
    public class DelegatInputModel
    {
        [ProtoMember(10)] 
        public int ArrangementId { get; set; }
        [ProtoMember(1)]
        public int? Id { get; set; }
        [ProtoMember(2)]
        [Required(ErrorMessage = "Delegatnummer er påkrevd")]
        public int? Delegatnummer { get; set; }

        [ProtoMember(3)]
        [Required(ErrorMessage = "Navn er påkrevd")]
        [StringLength(100)]
        public string? Navn { get; set; }
        [ProtoMember(4)]
        public string? Gruppe { get; set; }

        [ProtoMember(5)]
        [EmailAddress(ErrorMessage = "Ikke gyldig epost")]
        public string? Epost { get; set; }
        [ProtoMember(6)]
        [Phone(ErrorMessage = "Ikke gyldig telefonnummer")]
        public string? Telefon { get; set; }
    }

}