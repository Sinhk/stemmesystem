using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Stemmesystem.Core.Models
{

    [ProtoContract]
    [ProtoInclude(13, typeof(AdminVoteringDto))]
    public record VoteringDto
    {
        private VoteringDto()
        {
            Tittel = null!;
        }
        public VoteringDto(string tittel)
        {
            Tittel = tittel;
        }

        [ProtoMember(1)]
        public int Id { get; init; }
        [ProtoMember(2)]
        public string Tittel { get; init; } 
        [ProtoMember(3)]
        public string? Beskrivelse { get; init; }
        [ProtoMember(4)]
        public int KanVelge { get; init; }
        [ProtoMember(5)]
        public DateTime? StartTid { get; init; }
        [ProtoMember(6)]
        public DateTime? SluttTid { get; set; }
        [ProtoMember(7)]
        public int SakId { get; init; }

        [ProtoMember(8, IsRequired = true)] 
        public List<ValgDto> Valg { get; init; } = [];
        [ProtoMember(9)]
        public bool Aktiv { get; set; }
        [ProtoMember(10)]
        public bool Lukket { get; init; }
        [ProtoMember(11)]
        public bool Publisert { get; init; }
        [ProtoMember(12)]
        public bool Hemmelig { get; set; }

        public bool Startet => StartTid.GetValueOrDefault() > default(DateTime);
    }
    [ProtoContract]
    public record AdminVoteringDto : VoteringDto
    {
        [ProtoMember(1, IsRequired = true)] public List<DelegatDto> AvgitStemme { get; init; } = [];
        [ProtoMember(2, IsRequired = true)] public List<StemmeDto> Stemmer { get; init; } = [];

        private AdminVoteringDto() : this(tittel:null!)
        {
        }
        
        public AdminVoteringDto(string tittel) : base(tittel)
        {
        }
    }
    
    public record SakInfoDto(int Id, string Tittel, string Beskrivelse);


    [ProtoContract]
    public record VoteringResultatDto
    {
        [ProtoMember(1)]
        public int Id { get; init; }
        [ProtoMember(2)]
        public required string Tittel { get; init; }
        [ProtoMember(3)] public string? Beskrivelse { get; init; }
        [ProtoMember(4, IsRequired = true)] public List<StemmeDto> Stemmer { get; init; } = new();
        [ProtoMember(5, IsRequired = true)] public List<ValgDto> Valg { get; init; } = new();
        [ProtoMember(6)] public required string SakNavn { get; init; } 
        [ProtoMember(7)] public required string SakNummer { get; init; }
    }

    [ProtoContract]
    public record ValgDto
    {
        [ProtoMember(1)]
        public Guid Id { get; init; }
        [ProtoMember(2, IsRequired = true)]
        public required string Navn { get; set; }
        [ProtoMember(3)]
        public int? SortId { get; set; }
    }
    
    [ProtoContract]
    public record VoteringInputModel
    {
        [ProtoMember(1)]
        public int SakId { get; init; }
        [ProtoMember(2)]
        public int? Id { get; init; }

        [ProtoMember(4, IsRequired = true), Required] 
        public required string Tittel { get; set; }
        [ProtoMember(5)]
        public string? Beskrivelse { get; set; }
        [ProtoMember(6)]
        public bool Hemmelig { get; set; }
        [ProtoMember(7)]
        [DefaultValue(1)]
        [Required, Range(1,int.MaxValue,ErrorMessage = "\"Kan velge\" må være 1 eller mer") ]
        public int KanVelge { get; set; } =1;
        
        [ProtoMember(8, IsRequired = true)]
        public List<ValgDto> Valg { get; set; } = [];

        public bool Startet { get; set; }


    }

}