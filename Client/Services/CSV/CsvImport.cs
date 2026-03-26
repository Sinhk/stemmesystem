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

        public async Task<IEnumerable<DelegateInputModel>> ReadDelegates(TextReader reader)
        {
            using var csv = new CsvReader(reader, _csvConfiguration);
            
            var list = new List<CsvDelegate>();
            await foreach (var delegateRecord in csv.GetRecordsAsync<CsvDelegate>())
            {
                list.Add(delegateRecord);
            }

            return _mapper.Map<List<DelegateInputModel>>(list);
        }

        public async Task<IEnumerable<CaseInputModel>> ReadCases(TextReader reader, int arrangementId)
        {
            using var csv = new CsvReader(reader, _csvConfiguration);
            csv.Context.RegisterClassMap<CsvCaseMap>();
            var list = new List<CaseInputModel>();
            await foreach (var csvCase in csv.GetRecordsAsync<CsvCase>())
            {
                list.Add(new CaseInputModel
                {
                    ArrangementId = arrangementId,
                    Number = csvCase.Number,
                    Title = csvCase.Title,
                    Description = csvCase.Description
                    , Ballots = new List<BallotInputModel>
                    {
                        new()
                        {
                            Title = csvCase.Ballot ?? "" 
                            , Description = csvCase.BallotDescription
                            , Secret = csvCase.SecretBallot.GetValueOrDefault()
                            , MaxChoices = csvCase.MaxChoices ?? 1
                            , Choices = new List<ChoiceDto>(csvCase.Choices?.Select(v => new ChoiceDto
                            {
                                Id = new Guid()
                                , Name = v.Trim(' ','"')
                            }) ?? Enumerable.Empty<ChoiceDto>() )
                        }
                    }
                });
                
            }

            return list;
        }

        public static string CaseFormat
        {
            get
            {
                using var writer = new StringWriter();
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<CsvCaseMap>();
                csv.WriteHeader<CsvCase>();
                csv.NextRecord();
                csv.WriteRecord(new CsvCase()
                {
                    Title = "Import sak",
                    Description = "beskrivelse",
                    SecretBallot = true,
                    Number = "1.2.3",
                    Ballot = "kun 1 votering",
                    BallotDescription = "med beskrivelse",
                    Choices = new List<string>{"en,eller,flere"},
                    MaxChoices = 1
                });
                csv.Flush();
                return writer.ToString();
            }
        }
    }

}