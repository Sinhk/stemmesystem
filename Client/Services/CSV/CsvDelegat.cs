namespace Stemmesystem.Web.Services.CSV
{
    public record CsvDelegate
    {
        public int DelegateNumber { get; set; }
        public string? Name { get; set; }
        public string? Group { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        
    }


}