using AutoMapper;
using Stemmesystem.Data;
using Stemmesystem.Web.Models;
using System.Linq;
using Stemmesystem.Web.Services.CSV;

namespace Stemmesystem.Web
{
    internal class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<ArrangementModel, Arrangement>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Aktiv, o => o.Ignore())
            .ForMember(d => d.Saker, o => o.Ignore())
            .ForMember(d => d.Delegater, o => o.Ignore())
            .ReverseMap()
            ;

            CreateMap<Delegat, DelegatModel>()
                .ReverseMap();

            CreateMap<Sak, SakInputModel>()
                .ReverseMap();

            CreateMap<Votering, VoteringModel>()
                .ReverseMap();
            
            CreateMap<Valg, ValgModel>()
                .ReverseMap();

            CreateMap<CsvDelegat, DelegatModel>()
                .ForMember(d=> d.Id, o=> o.Ignore());

        }
    }
}