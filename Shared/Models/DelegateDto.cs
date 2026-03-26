using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Stemmesystem.Shared.Models
{
    [ProtoContract]
    [ProtoInclude(8,typeof(AdminDelegateDto))]
    public record DelegateDto
    {
        [ProtoMember(1)]
        public int Id { get; internal set; }
        [ProtoMember(2)]
        public int? DelegateNumber { get; set; }
        [ProtoMember(3)]
        public string? Name { get; set; }
        [ProtoMember(4)]
        public string? Group { get; set; }
        [ProtoMember(5)]
        public string? Email { get; set; }
        [ProtoMember(6)]
        public string? Phone { get; set; }
        [ProtoMember(7)]
        public int ArrangementId { get; set; }
    }

    [ProtoContract]
    public record AdminDelegateDto : DelegateDto
    {
        [ProtoMember(9)]
        public string? DelegateCode { get; init; }
        
        [ProtoMember(10)]
        public DateTime? EmailSentAt { get; set; }
        [ProtoMember(11)]
        public DateTime? SmsSentAt { get; set; }
        
        [ProtoMember(12)]
        public bool Present { get; set; }
    }
    
    [ProtoContract]
    public class DelegateInputModel
    {
        [ProtoMember(10)] 
        public int ArrangementId { get; set; }
        [ProtoMember(1)]
        public int? Id { get; set; }
        [ProtoMember(2)]
        [Required(ErrorMessage = "Delegatnummer er påkrevd")]
        public int? DelegateNumber { get; set; }

        [ProtoMember(3)]
        [Required(ErrorMessage = "Navn er påkrevd")]
        [StringLength(100)]
        public string? Name { get; set; }
        [ProtoMember(4)]
        public string? Group { get; set; }

        [ProtoMember(5)]
        [EmailAddress(ErrorMessage = "Ikke gyldig epost")]
        public string? Email { get; set; }
        [ProtoMember(6)]
        [Phone(ErrorMessage = "Ikke gyldig telefonnummer")]
        public string? Phone { get; set; }
    }

}