using AutoMapper;
using Stemmesystem.Data.Entities;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.Models;
using DelegateEntity = Stemmesystem.Data.Entities.Delegate;

namespace Stemmesystem.Api;

public class ApiAutoMapperProfile : Profile
{
    public ApiAutoMapperProfile()
    {
        CreateMap<Arrangement, ArrangementDto>();
        CreateMap<Arrangement, ArrangementInfo>()
            .ForMember(d => d.BallotsCount, opt => opt.MapFrom(s => s.Cases.Sum(c => c.Ballots.Count)));
        CreateMap<ArrangementInputModel, Arrangement>(MemberList.Source);
            
        CreateMap<DelegateEntity, DelegateDto>()
            .ReverseMap();
        
        CreateMap<DelegateEntity, AdminDelegateDto>()
            .ReverseMap();

        CreateMap<Case, CaseDto>();
        CreateMap<Case, AdminCaseDto>();
        CreateMap<Case, CaseInfoDto>();
        CreateMap<CaseInputModel, Case>(MemberList.Source);
            

        CreateMap<Vote, VoteDto>()
            .ReverseMap();

        CreateMap<Ballot, BallotDto>();
        CreateMap<Ballot, AdminBallotDto>();
        CreateMap<Ballot, BallotResultDto>()
            .ForMember(v => v.CaseName, opt => opt.MapFrom(s => s.Case.Title))
            .ForMember(v => v.CaseNumber, opt => opt.MapFrom(s => s.Case.Number))
            .ForMember(v => v.CastVoteCount, opt => opt.MapFrom(s => s.VotedDelegates.Count));
        CreateMap<Ballot, BallotInputModel>()
            .ForMember(s => s.Started, o => o.Ignore())
            .ReverseMap();
            
        CreateMap<Choice, ChoiceDto>()
            .ReverseMap();

        CreateMap<DelegateInputModel, DelegateEntity>(MemberList.Source);

    }

}