using CSharpVitamins;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data.Models;
using Stemmesystem.Web.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stemmesystem.Data
{
    public record Stemme
    {
        internal Guid Id { get; private set; }
        public Guid ValgId { get; private set; }

        private Stemme() { }
        public Stemme(Guid valgId)
        {
            ValgId = valgId;
        }

        //public Valg Valg { get => valg ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Valg)); private set => valg = value; }
        public Delegat? Delegat { get; internal set; }
        public int DelegatId { get; internal set; }
    }

    public class Delegat
    {
        private Arrangement? arrangement;
        private List<Votering> harStemmtI = new List<Votering>();

        public int Id { get; private set; }
        public string DelegatKode { get; set; }
        public int Delegatnummer { get; set; }
        public string? Navn { get; set; }
        public string? Epost { get; set; }
        public string? Telefon { get; set; }

        public int ArrangementId { get; set; }
        public Arrangement Arrangement { get => arrangement ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Arrangement)); set => arrangement = value; }

        public IEnumerable<Votering> HarStemmtI => harStemmtI;
        internal Delegat(int delegatnummer, string? navn, string? delegatKode = null)
        {
            Delegatnummer = delegatnummer;
            Navn = navn;

            delegatKode ??= ShortGuid.NewGuid();
            DelegatKode = delegatKode.ToString();
        }
    }

    public class Gruppe
    {
        public int Id { get; private set; }
        public string Navn { get; set; }
        public IList<Delegat> Delegater { get; set; } = new List<Delegat>();

        public Gruppe(string navn)
        {
            Navn = navn;
        }

    }

    public class Arrangement
    {
        public int Id { get; internal set; }

        public Votering? FinnVotering(int voteringId)
        {
            return Saker.SelectMany(s => s.Voteringer).Where(v => v.Id == voteringId).FirstOrDefault();
        }

        public string Navn { get; init; }

        public string? Beskrivelse { get; set; }
        public bool Aktiv { get; set; }


        public IList<Sak> Saker { get; set; } = new List<Sak>();
        public IList<Delegat> Delegater { get; set; } = new List<Delegat>();
        public Arrangement(string navn)
        {
            Navn = navn;
            Aktiv = true;
        }

        public void LeggTil(Sak sak)
        {
            Saker.Add(sak);
        }

        public void NyDelegat(int nummer, string navn, string? kode = null)
        {
            var delegat = new Delegat(nummer, navn, kode);
            Delegater.Add(delegat);
        }

        public Votering? AktivVotering()
        {
            return Saker.SelectMany(s => s.Voteringer).Where(v => v.Aktiv).FirstOrDefault();
        }

        public Delegat NyDelegat(NyDelegatModel model, string delegatkode)
        {
            var delegat = new Delegat(model.Nummer, model.Navn, delegatkode)
            {
                Epost = model.Epost,
                Telefon = model.Telefon
            };
            Delegater.Add(delegat);
            return delegat;
        }
    }

    public class Sak
    {
        public int Id { get; internal set; }
        public int Nummer { get; private set; }
        public string Tittel { get; private set; }

        public int ArrangementId { get; internal set; }

        public void LeggTil(params Votering[] voteringer)
        {
            foreach (var votering in voteringer)
            {
                Voteringer.Add(votering);
            }
        }

        public Sak(int nummer, string tittel)
        {
            Nummer = nummer;
            Tittel = tittel;
        }

        public string? Beskrivelse { get; set; }

        public IList<Votering> Voteringer { get; set; } = new List<Votering>();

    }

    public class Votering
    {
        protected List<Valg> valg = new List<Valg>();
        protected List<Stemme> stemmer = new List<Stemme>();
        protected List<Delegat> avgitStemme = new List<Delegat>();
        private Sak? sak;

        public int Id { get; internal set; }
        public string Tittel { get; init; }
        public bool Hemmelig { get; set; }
        public bool Aktiv { get; set; } = false;
        public DateTimeOffset StartTid { get; set; }
        public DateTimeOffset SluttTid { get; set; }
        public IEnumerable<Valg> Valg => valg;
        public IEnumerable<Stemme> Stemmer => stemmer;
        public IEnumerable<Delegat> AvgitStemme => avgitStemme;

        public Sak Sak { get => sak! /*?? throw new InvalidOperationException("Uninitialized property: " + nameof(Sak))*/; set => sak = value; }
        public int SakId { get; internal set; }
        public Votering(string tittel, bool hemmelig)
        {
            Tittel = tittel;
            Hemmelig = hemmelig;
        }

        public Votering(string tittel, bool hemmelig, params string[] valgtekst) : this (tittel, hemmelig)
        {
            var i = 0;
            foreach (var tekst in valgtekst)
            {
                valg.Add(new(tekst, i));
                i++;
            }
        }

        public void StartVotering()
        {
            Aktiv = true;
            StartTid = DateTimeOffset.Now;
        }

        public void AvsluttVotering()
        {
            Aktiv = false;
            SluttTid = DateTimeOffset.Now;
        }

        public Stemme RegistrerStemme(Guid valgId, Delegat delegat, Stemme? gammelStemme = null)
        {
            if(gammelStemme != null)
            {
                stemmer.Remove(gammelStemme);
            }
            else if (avgitStemme.Any(d => d.Id == delegat.Id))
                throw new StemmeException("Delegat har allerede stemmt");

            Valg? v;
            if (valgId == Konstanter.BlankStemme)
                v = new Valg("Blankt");
            else
                v = valg.SingleOrDefault(v => v.Id == valgId);
            if (v == null)
                throw new StemmeException("Ugyldig valg");
            Stemme? stemme = new(valgId);
            if (!Hemmelig)
                stemme.DelegatId = delegat.Id;

            avgitStemme.Add(delegat);
            stemmer.Add(stemme);
            return stemme;
        }
    }

    public class EnkelVotering : Votering
    {
        public EnkelVotering(string tittel, bool hemmelig = false) : base(tittel, hemmelig)
        {
            valg.Add(new("For"));
            valg.Add(new("Mot"));
        }
        public EnkelVotering(string tittel, bool hemmelig = false, params string[] valgtekst) : base(tittel, hemmelig)
        {
            var i = 0;
            foreach(var tekst in valgtekst)
            {
                valg.Add(new(tekst,i));
                i++;
            }
        }
    }

    public class Flervalgsvotering : Votering
    {
        public Flervalgsvotering(string tittel, IEnumerable<string> valg, bool hemmelig = true) : base(tittel, hemmelig)
        {
            this.valg = valg.Select((v,i) => new Valg(v,i)).ToList();
        }
    }

    [Owned]
    public class Valg
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }
        public string Navn { get; init; }
        public int? SortId { get; set; }

        public Valg(string navn, int? sortId = null)
        {
            Navn = navn;
            SortId = sortId;
        }
    }
}
