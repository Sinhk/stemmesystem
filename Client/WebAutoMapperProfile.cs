using AutoMapper;
using Stemmesystem.Client.Services.CSV;
using Stemmesystem.Shared.Models;
using Stemmesystem.Web.Services.CSV;

namespace Stemmesystem.Client
{
    internal class WebAutoMapperProfile : Profile
    {
        public WebAutoMapperProfile()
        {
            CreateMap<CsvDelegate, DelegateInputModel>()
                .ForMember(d=> d.Id, o=> o.Ignore())
                .ForMember(d=> d.ArrangementId, o=> o.Ignore())
                ;
            
            CreateMap<CsvCase, CaseInputModel>()
                .ForMember(d=> d.Id, o=> o.Ignore())
                .ForMember(d=> d.ArrangementId, o=> o.Ignore())
                .ForMember(d=> d.Ballots, o=> o.Ignore())
                ;

            CreateMap<DelegateDto, DelegateInputModel>()
                .ForMember(d => d.Id, opt => opt.Condition(s => s.Id != default));
            CreateMap<ArrangementDto, ArrangementInputModel>();
            CreateMap<CaseDto, CaseInputModel>();
            CreateMap<BallotDto, BallotInputModel>();

        }
    }
}