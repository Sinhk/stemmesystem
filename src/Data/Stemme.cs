using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data.Models;
using Stemmesystem.Tools;
using Stemmesystem.Web.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

        internal string? RevoteKey { get; set; }
    }

    public class Delegat
    {
        private Arrangement? arrangement;
        private List<Votering> harStemmtI = new List<Votering>();

        public int Id { get; private set; }
        public string Delegatkode { get; set; }
        public int Delegatnummer { get; set; }
        public string? Navn { get; set; }
        public string? Epost { get; set; }
        public string? Telefon { get; set; }

        public int ArrangementId { get; set; }
        public Arrangement Arrangement { get => arrangement ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Arrangement)); set => arrangement = value; }

        public IEnumerable<Votering> HarStemmtI => harStemmtI;
        internal Delegat(int delegatnummer, string? navn, string? delegatkode = null)
        {
            Delegatnummer = delegatnummer;
            Navn = navn;

            delegatkode ??= RngKeyGenerator.GenerateKey(4);
            Delegatkode = delegatkode.ToString();
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
        public string? Logo { get; set; }
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
        private Arrangement? arrangement;

        public int Id { get; internal set; }
        public string Nummer { get; private set; }
        public string Tittel { get; private set; }

        public Arrangement Arrangement { get => arrangement ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Arrangement)); internal set => arrangement = value; }
        public int ArrangementId { get; internal set; }

        public void LeggTil(params Votering[] voteringer)
        {
            foreach (var votering in voteringer)
            {
                Voteringer.Add(votering);
            }
        }

        public Sak(string nummer, string tittel)
        {
            Nummer = nummer;
            Tittel = tittel;
        }
        public Sak(int nummer, string tittel) : this(nummer.ToString(), tittel)
        {
        }

        public string? Beskrivelse { get; set; }

        public IList<Votering> Voteringer { get; set; } = new List<Votering>();

    }

    public class Votering
    {
        private List<Valg> _valg = new();
        private List<Stemme> _stemmer = new();
        private List<Delegat> _avgitStemme = new();
        private Sak? sak;

        public int Id { get; internal set; }
        public string Tittel { get; init; }
        public bool Hemmelig { get; set; }
        public bool Aktiv { get; set; } = false;

        public int KanVelge { get; set; } = 1;

        public DateTimeOffset StartTid { get; set; }
        public DateTimeOffset SluttTid { get; set; }
        public IReadOnlyList<Valg> Valg => _valg;
        public IReadOnlyList<Stemme> Stemmer => _stemmer;
        public IReadOnlyList<Delegat> AvgitStemme => _avgitStemme;

        public Sak Sak { get => sak ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Sak)); set => sak = value; }
        public int SakId { get; internal set; }
        public Votering(string tittel, bool hemmelig)
        {
            Tittel = tittel;
            Hemmelig = hemmelig;
        }

        public Votering(string tittel, bool hemmelig, params string[] valgtekst) : this(tittel, hemmelig)
        {
            var i = 0;
            foreach (var tekst in valgtekst)
            {
                _valg.Add(new(tekst, i));
                i++;
            }
        }
        public Votering(string tittel, bool hemmelig, int? kanVelge, params string[] valgtekst) : this (tittel, hemmelig, valgtekst)
        {
            KanVelge = kanVelge.GetValueOrDefault(1);
        }

        public static Votering EnkelVotering(string tittel, bool hemmelig = false)
        {
            Votering v = new(tittel, hemmelig);
            v._valg.Add(new("For"));
            v._valg.Add(new("Mot"));
            return v;
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

        public (List<Stemme> stemmer, string key) RegistrerStemme(IEnumerable<Guid> valgIder, Delegat delegat,  string? revoteKey, IKeyHasher keyHasher)
        {
            List<Guid> idList = valgIder.ToList();
            if(idList.Count > 1 && idList.Contains(Konstanter.BlankStemme))
                throw new StemmeException("Kan ikke ha flere valg i en blank stemme");

            if(idList.Count > KanVelge)
                throw new StemmeException($"For mange valg {idList.Count}, bare {KanVelge} er lov per delegat");

            if (_avgitStemme.Any(d => d.Id == delegat.Id))
            {
                if (Hemmelig)
                {
                    if (revoteKey != null)
                    {
                        _stemmer.RemoveAll(s => keyHasher.VerifyHash(s.RevoteKey, revoteKey));
                    }
                    else
                    {
                        throw new StemmeException("Delegat har allerede stemmt");
                    }

                }
                else
                {
                    _stemmer.RemoveAll(s => s.DelegatId == delegat.Id);
                }
            }

            var key = RngKeyGenerator.GenerateKey(20, KeyType.FullAlphanumeric);
            List<Stemme> stemmer = new();
            foreach (var valgId in idList)
            {
                if (valgId != Konstanter.BlankStemme && _valg.All(v => v.Id != valgId))
                    throw new StemmeException("Ugyldig valg");
                Stemme stemme = new(valgId);
                if (!Hemmelig)
                    stemme.DelegatId = delegat.Id;
                stemme.RevoteKey = keyHasher.CreateHash(key);
                stemmer.Add(stemme);
            }

            _stemmer.AddRange(stemmer);
            _avgitStemme.Add(delegat);

            return (stemmer, key);
        }

        public TEntity RegistrerStemme<TEntity>(IEnumerable<Guid> valgIder, Delegat delegat, string? gammelStemme, IKeyHasher keyHasher) where TEntity : class
        {
            throw new NotImplementedException();
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
