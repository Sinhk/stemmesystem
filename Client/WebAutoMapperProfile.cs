using AutoMapper;
using Stemmesystem.Client.Services.CSV;
using Stemmesystem.Core.Models;
using Stemmesystem.Web.Services.CSV;

namespace Stemmesystem.Client;

internal sealed class WebAutoMapperProfile : Profile
{
    public WebAutoMapperProfile()
    {
        CreateMap<CsvDelegat, DelegatInputModel>()
            .ForMember(d=> d.Id, o=> o.Ignore());
            
        CreateMap<CsvSak, SakInputModel>()
            .ForMember(d=> d.Id, o=> o.Ignore());

        CreateMap<DelegatDto, DelegatInputModel>()
            .ForMember(d => d.Id, opt => opt.Condition(s => s.Id != default));
        CreateMap<ArrangementDto, ArrangementInputModel>();
        CreateMap<SakDto, SakInputModel>();
        CreateMap<VoteringDto, VoteringInputModel>();

    }
}