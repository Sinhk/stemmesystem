using Stemmesystem.Tools;
using Stemmesystem.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stemmesystem.Data
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

        public DateTimeOffset? StartTid { get; set; }
        public DateTimeOffset? SluttTid { get; set; }
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
            v._valg.Add(new("For"));
            v._valg.Add(new("Mot"));
            return v;
        }

        public void StartVotering()
        {
            if (Lukket)
                throw new StemmeException("Votering lukket, kan ikke startes igjen");
            Aktiv = true;
            StartTid = DateTimeOffset.Now;
        }

        public void AvsluttVotering()
        {
            Aktiv = false;
            SluttTid = DateTimeOffset.Now;
        }

        public List<Stemme> RegistrerStemme(IEnumerable<Guid> valgIder, Delegat delegat, string delegatkode, IKeyHasher keyHasher, NotificationManager notificationManager)
        {
            List<Guid> idList = valgIder.ToList();
            if(idList.Count > 1 && idList.Contains(Konstanter.BlankStemme))
                throw new StemmeException("Kan ikke ha flere valg i en blank stemme");
            
            if(idList.Count > KanVelge)
                throw new StemmeException($"For mange valg {idList.Count}, bare {KanVelge} er lov per delegat");

            var notifier = notificationManager.ForArrangement(Sak.ArrangementId);
            if (_avgitStemme.Any(d => d.Id == delegat.Id))
            {
                List<Stemme> fjernes = Hemmelig ? _stemmer.Where(s => keyHasher.VerifyHash(s.StemmeHash, delegatkode)).ToList() : _stemmer.Where(s => s.DelegatId == delegat.Id).ToList();

                _stemmer.RemoveAll(s=>  fjernes.Contains(s));
                fjernes.ForEach(s=> notifier.OnStemmeFjernet(new StemmeFjernetEvent(Id,s.Id)));
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

            stemmer.ForEach(s=> notifier.OnNyStemme(new NyStemmeEvent(Id,s)));
            notifier.OnHarStemt(new HarStemtEvent(Id,delegat.Id));
            return stemmer;
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
                Aktiv = true
                , Beskrivelse = Beskrivelse
                , KanVelge = KanVelge
                , SakId = SakId
                , _valg = new List<Valg>(_valg.Select(v => new Valg(v.Navn, v.SortId){Id = Guid.NewGuid()}))
            };
    }
}
