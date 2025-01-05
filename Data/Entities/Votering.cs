using System.Diagnostics.CodeAnalysis;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Models;
using Stemmesystem.Shared.Tools;

namespace Stemmesystem.Data.Entities
{
    public class Votering
    {
        private List<Valg> _valg = new();
        private List<Stemme> _stemmer = new();
        private List<Delegat> _avgitStemme = new();
        private Sak? sak;

        public int Id { get; internal set; }
        public string Tittel { get; set; }
        public string? Beskrivelse { get; set; }
        public bool Hemmelig { get; set; }
        public bool Aktiv { get; set; } = false;

        public int KanVelge { get; set; } = 1;

        public DateTime? StartTid { get; set; }
        public DateTime? SluttTid { get; set; }
        public IReadOnlyList<Valg> Valg => _valg;
        public List<Stemme> Stemmer => _stemmer;
        public IReadOnlyList<Delegat> AvgitStemme => _avgitStemme;

        public Sak Sak { get => sak ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Sak)); set => sak = value; }
        public int SakId { get; internal set; }
        public bool Lukket { get; set; }
        public bool Publisert { get; set; }

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
            v._valg.Add(new Valg("For"));
            v._valg.Add(new Valg("Mot"));
            return v;
        }

        public void StartVotering()
        {
            if (Lukket)
                throw new StemmeException("Votering lukket, kan ikke startes igjen");
            Aktiv = true;
            StartTid = DateTime.UtcNow;
        }

        public Valg LeggTilValg(string tekst, int? sortId = null)
        {
            var valg = new Valg(tekst, sortId ?? _valg.Count);
            _valg.Add(valg);
            return valg;
        }

        [MemberNotNull(nameof(SluttTid))]
        public void AvsluttVotering(int delegaterTilstede)
        {
            Aktiv = false;
            SluttTid = DateTime.UtcNow;
            DelegaterTilstede = delegaterTilstede;
        }

        /// <summary>
        /// Antall delegater som var tilstede under voteringen
        /// </summary>
        public int? DelegaterTilstede { get; set; }

        public (List<Stemme> stemmer, List<Stemme>? fjernes) RegistrerStemme(IEnumerable<Guid> valgIder, Delegat delegat, string delegatkode, IKeyHasher keyHasher)
        {
            List<Guid> idList = valgIder.ToList();
            if(idList.Count > 1 && idList.Contains(Konstanter.BlankStemme))
                throw new StemmeException("Kan ikke ha flere valg i en blank stemme");
            
            if(idList.Count > KanVelge)
                throw new StemmeException($"For mange valg {idList.Count}, bare {KanVelge} er lov per delegat");

            List<Stemme>? fjernes = null;
            if (_avgitStemme.Any(d => d.Id == delegat.Id))
            {
                fjernes = Hemmelig ? _stemmer.Where(s => keyHasher.VerifyHash(s.StemmeHash, delegatkode)).ToList() : _stemmer.Where(s => s.DelegatId == delegat.Id).ToList();
                _stemmer.RemoveAll(s=>  fjernes.Contains(s));
            }

            List<Stemme> stemmer = new(); 
            foreach (var valgId in idList)
            {
                if (valgId != Konstanter.BlankStemme && _valg.All(v => v.Id != valgId))
                    throw new StemmeException("Ugyldig valg");
                Stemme stemme = new(valgId);
                if (!Hemmelig)
                    stemme.DelegatId = delegat.Id;
                stemme.StemmeHash = keyHasher.CreateHash(delegatkode);
                stemmer.Add(stemme);
            }

            _stemmer.AddRange(stemmer);
            _avgitStemme.Add(delegat);

            return (stemmer,fjernes);
        }

        public void OppdaterValg(IEnumerable<ValgDto> valg)
        {
            foreach (var v in valg)
            {
                var existing = _valg.FirstOrDefault(vv => vv.Id == v.Id);
                if (existing == null)
                {
                    LeggTilValg(v.Navn, v.SortId);
                    continue;
                }

                existing.Navn = v.Navn;
                existing.SortId = v.SortId;
            }
        }

        public void LukkVotering()
        {
            Lukket = true;
        }

        public void PubliserVotering()
        {
            if (!Lukket)
                throw new StemmeException("Votering må lukkes før den kan publiseres");
            Publisert = true;
        }

        public Votering Kopier() =>
            new(Tittel, Hemmelig)
            {
                Aktiv = false
                , Beskrivelse = Beskrivelse
                , KanVelge = KanVelge
                , SakId = SakId
                , _valg = new List<Valg>(_valg.Select(v => new Valg(v.Navn, v.SortId){Id = Guid.NewGuid()}))
            };
    }
}
