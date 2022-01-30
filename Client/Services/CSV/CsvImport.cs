using System.Globalization;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Stemmesystem.Shared.Models;
using Stemmesystem.Web.Services.CSV;

namespace Stemmesystem.Client.Services.CSV
{
    public class CsvImport
    {
        private readonly IMapper _mapper;

        public CsvImport(IMapper mapper)
        {
            _mapper = mapper;
        }
        
        private readonly CsvConfiguration _csvConfiguration = new(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
            HeaderValidated = null,
            MissingFieldFound = null
        };

        public async Task<IEnumerable<DelegatInputModel>> LesDelagater(TextReader reader)
        {
            using var csv = new CsvReader(reader, _csvConfiguration);
            
            var list = new List<CsvDelegat>();
            await foreach (var delegat in csv.GetRecordsAsync<CsvDelegat>())
            {
                list.Add(delegat);
            }

            return _mapper.Map<List<DelegatInputModel>>(list);
        }

        public async Task<IEnumerable<SakInputModel>> LesSaker(TextReader reader, int arrangementId)
        {
            using var csv = new CsvReader(reader, _csvConfiguration);
            csv.Context.RegisterClassMap<CsvSakMap>();
            var list = new List<SakInputModel>();
            await foreach (var sak in csv.GetRecordsAsync<CsvSak>())
            {
                Console.WriteLine(sak);
                list.Add(new SakInputModel
                {
                    ArrangementId = arrangementId,
                    Nummer = sak.Nummer,
                    Tittel = sak.Tittel,
                    Beskrivelse = sak.Beskrivelse
                    , Voteringer = new List<VoteringInputModel>
                    {
                        new()
                        {
                            Tittel = sak.Votering ?? "" 
                            , Beskrivelse = sak.VoteringBeskrivelse
                            , Hemmelig = sak.HemmeligVotering.GetValueOrDefault()
                            , KanVelge = sak.KanVelge ?? 1
                            , Valg = new List<ValgDto>(sak.Valg?.Select(v => new ValgDto
                            {
                                Id = new Guid()
                                , Navn = v
                            }) ?? Enumerable.Empty<ValgDto>() )
                        }
                    }
                });
                
            }

            return list;
        }

        public static string SakFormat
        {
            get
            {
                using var writer = new StringWriter();
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<CsvSakMap>();
                csv.WriteHeader<CsvSak>();
                csv.NextRecord();
                csv.WriteRecord(new CsvSak()
                {
                    Tittel = "Import sak",
                    Beskrivelse = "beskrivelse",
                    HemmeligVotering = true,
                    Nummer = "1.2.3",
                    Votering = "kun 1 votering",
                    VoteringBeskrivelse = "med beskrivelse",
                    Valg = new List<string>{"en,eller,flere"},
                    KanVelge = 1
                });
                csv.Flush();
                return writer.ToString();
            }
        }
    }

}