namespace Stemmesystem.Web.Services.CSV
{
    public record CsvDelegat
    {
        public int Delegatnummer { get; set; }
        public string Navn { get; set; }
        public string Gruppe { get; set; }
        public string Epost { get; set; }
        public string Telefon { get; set; }
        
    }
}