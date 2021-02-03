using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Stemmesystem.Web.Models;

namespace Stemmesystem.Web.Services.CSV
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
            PrepareHeaderForMatch = (header, index) => header.ToLower(),
            HeaderValidated = null,
            MissingFieldFound = null
        };

        public async Task<IEnumerable<DelegatModel>> LesDelagater(TextReader reader)
        {
            using var csv = new CsvReader(reader, _csvConfiguration);
            
            var list = new List<CsvDelegat>();
            await foreach (var delegat in csv.GetRecordsAsync<CsvDelegat>())
            {
                list.Add(delegat);
            }

            return _mapper.Map<List<DelegatModel>>(list);
        }
    }
}