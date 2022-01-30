using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stemmesystem.Data.Models
{
    public record NyDelegatModel(int Nummer)
    {
        public string? Navn { get; init; }
        public string? Telefon { get; init; }
        public string? Epost { get; init; }
    }
}
