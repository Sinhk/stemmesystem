using Stemmesystem.Shared.Models;

namespace Stemmesystem.Client;

public class DelegatFormModel
{
    public DelegatFormModel() { }
    public DelegatFormModel(DelegatInputModel delegat)
    {
        Id = delegat.Id;
        Delegatnummer = delegat.Delegatnummer;
        Navn = delegat.Navn;
        Gruppe = delegat.Gruppe;
        Epost = delegat.Epost;
        Telefon = delegat.Telefon;
    }

    public DelegatFormModel(DelegatDto delegat)
    {
        Id = delegat.Id;
        Delegatnummer = delegat.Delegatnummer;
        Navn = delegat.Navn;
        Gruppe = delegat.Gruppe;
        Epost = delegat.Epost;
        Telefon = delegat.Telefon;
    }

    public int? Id { get; set; }
    public int? Delegatnummer { get; set; }
    public string? Navn { get; set; }
    public string? Gruppe { get; set; }
    public string? Epost { get; set; }
    public string? Telefon { get; set; }
        
    public DelegatInputModel ToInputModel(int arrangementId) =>
        new()
        {
            Id = Id,
            Delegatnummer = Delegatnummer,
            Navn = Navn,
            Gruppe = Gruppe,
            Epost = Epost,
            Telefon = Telefon,
            ArrangementId = arrangementId
        };

    public void ApplyChanges(AdminDelegatDto delegat)
    {
        Id = delegat.Id;
        Delegatnummer = delegat.Delegatnummer;
        Navn = delegat.Navn;
        Gruppe = delegat.Gruppe;
        Epost = delegat.Epost;
        Telefon = delegat.Telefon;
    }
}