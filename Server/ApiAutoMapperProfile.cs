using AutoMapper;
using StemmeSystem.Data.Entities;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Api;

public class ApiAutoMapperProfile : Profile
{
    public ApiAutoMapperProfile()
    {
        /*CreateMap<ArrangementDto, Arrangement>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Aktiv, o => o.Ignore())
            .ForMember(d => d.Saker, o => o.Ignore())
            .ForMember(d => d.Delegater, o => o.Ignore())
            .ReverseMap()
            ;
            */

        CreateMap<Arrangement, ArrangementDto>();
        CreateMap<Arrangement, ArrangementInfo>()
            .ForMember(d => d.VoteringerCount, opt => opt.MapFrom(s => s.Saker.Sum(sak => sak.Voteringer.Count)));
        CreateMap<ArrangementInputModel, Arrangement>(MemberList.Source);
            
        CreateMap<Delegat, DelegatDto>()
            .ReverseMap();
        
        CreateMap<Delegat, AdminDelegatDto>()
            .ReverseMap();

        CreateMap<Sak, SakDto>();
        CreateMap<Sak, AdminSakDto>();
        CreateMap<Sak, SakInfoDto>();
        CreateMap<SakInputModel, Sak>(MemberList.Source);
            

        CreateMap<Stemme, StemmeDto>()
            .ReverseMap();

        CreateMap<Votering, VoteringDto>();
        CreateMap<Votering, AdminVoteringDto>();
        CreateMap<Votering, VoteringResultatDto>()
            .ForMember(v =>v.SakNavn, opt => opt.MapFrom(s => s.Sak.Tittel));
        CreateMap<Votering, VoteringInputModel>()
            .ForMember(s => s.Startet, o => o.Ignore())
            .ReverseMap();;
        /*CreateMap<Votering, VoteringDto>()
            .ForMember(v=> v.Id,opt =>
            {
                opt.MapFrom(s => s.Id);
                opt.Condition(v => v.Id != default);
            })
            .ReverseMap();
            */
            
        CreateMap<Valg, ValgDto>()
            .ReverseMap();

        CreateMap<DelegatInputModel, Delegat>(MemberList.Source);

    }

}