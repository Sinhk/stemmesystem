using AutoMapper;
using Stemmesystem.Shared.Models;
using Stemmesystem.Web.Services.CSV;

namespace Stemmesystem.Web
{
    internal class WebAutoMapperProfile : Profile
    {
        public WebAutoMapperProfile()
        {
            CreateMap<CsvDelegat, DelegatInputModel>()
                .ForMember(d=> d.Id, o=> o.Ignore());

            CreateMap<DelegatDto, DelegatInputModel>()
                .ForMember(d => d.Id, opt => opt.Condition(s => s.Id != default));
            CreateMap<ArrangementDto, ArrangementInputModel>();
            CreateMap<SakDto, SakInputModel>();
            CreateMap<VoteringDto, VoteringInputModel>();

        }
    }
}