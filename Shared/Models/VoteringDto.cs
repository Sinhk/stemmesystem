using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Stemmesystem.Shared.Models
{

    [ProtoContract]
    [ProtoInclude(13, typeof(AdminVoteringDto))]
    public record VoteringDto
    {
        [ProtoMember(1)]
        public int Id { get; init; }
        [ProtoMember(2)]
        public string? Tittel { get; init; }
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

        [ProtoMember(8)] public List<ValgDto> Valg { get; init; } = new();
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
        [ProtoMember(1)] public List<DelegatDto> AvgitStemme { get; init; } = new();
        [ProtoMember(2)] public List<StemmeDto> Stemmer { get; init; } = new();

    }
    
    public record SakInfoDto(int Id, string Tittel, string Beskrivelse);


    [ProtoContract]
    public record VoteringResultatDto
    {
        [ProtoMember(1)]
        public int Id { get; init; }
        [ProtoMember(2)]
        public string Tittel { get; init; }
        [ProtoMember(3)] public string Beskrivelse { get; init; }
        [ProtoMember(4)] public List<StemmeDto> Stemmer { get; init; } = new();
        [ProtoMember(5)] public List<ValgDto> Valg { get; init; } = new();
        [ProtoMember(6)] public string SakNavn { get; init; }
        [ProtoMember(7)] public string SakNummer { get; init; }
        
    }

    /*
    [ProtoContract]
    public record VoteringDto
    {
        internal VoteringDto() { }
        
        public VoteringDto(int id, string tittel, SakDto sak, List<StemmeDto> stemmer)
        {
            Id = id;
            Tittel = tittel;
            Sak = sak;
            Stemmer = stemmer;
        }

        [ProtoMember(1)]
        public int Id { get; internal set; }
        [ProtoMember(2)]
        public string Tittel { get; init; }
        [ProtoMember(3)]
        public string? Beskrivelse { get; set; }
        [ProtoMember(4)]
        public bool Hemmelig { get; set; }
        [ProtoMember(5)]
        public int KanVelge { get; set; } 

        [ProtoMember(6)]
        public List<ValgDto> Valg { get; set; } = new();

        [ProtoMember(7)]
        public DateTimeOffset? StartTid { get; init; }

        [ProtoMember(8)]
        public DateTimeOffset? SluttTid { get; set; }
        [ProtoMember(9)]
        public SakDto Sak { get; init; }
        [ProtoMember(10)]
        public bool Publisert { get; set; }

        [ProtoMember(11)]
        public List<StemmeDto> Stemmer { get; init; }
        [ProtoMember(12)]
        public bool Aktiv { get; set; }

        [ProtoMember(13)]
        public bool Lukket { get; init; }
/*
        [ProtoMember(14)]
        public ICollection<DelegatDto> AvgitStemme { get; set; }
        */
    /*
    }
*/
    
    [ProtoContract]
    public record ValgDto
    {
        [ProtoMember(1)]
        public Guid Id { get; init; }
        [ProtoMember(2)]
        public string Navn { get; set; } = null!;
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

        [ProtoMember(4), Required] 
        public string Tittel { get; set; } = null!;
        [ProtoMember(5)]
        public string? Beskrivelse { get; set; }
        [ProtoMember(6)]
        public bool Hemmelig { get; set; }
        [ProtoMember(7)]
        [DefaultValue(1)]
        [Required, Range(1,int.MaxValue,ErrorMessage = "\"Kan velge\" må være 1 eller mer") ]
        public int KanVelge { get; set; } =1;
        
        [ProtoMember(8)]
        public List<ValgDto> Valg { get; set; } = new();

        public bool Startet { get; set; }


    }

}