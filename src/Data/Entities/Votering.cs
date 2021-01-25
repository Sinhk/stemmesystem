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
        public string Tittel { get; init; }
        public bool Hemmelig { get; set; }
        public bool Aktiv { get; set; } = false;

        public int KanVelge { get; set; } = 1;

        public DateTimeOffset StartTid { get; set; }
        public DateTimeOffset SluttTid { get; set; }
        public IReadOnlyList<Valg> Valg => _valg;
        public List<Stemme> Stemmer => _stemmer;
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

        public (List<Stemme> stemmer, string key) RegistrerStemme(IEnumerable<Guid> valgIder, Delegat delegat, string? revoteKey, IKeyHasher keyHasher, NotificationManager notificationManager)
        {
            List<Guid> idList = valgIder.ToList();
            if(idList.Count > 1 && idList.Contains(Konstanter.BlankStemme))
                throw new StemmeException("Kan ikke ha flere valg i en blank stemme");
            
            if(idList.Count > KanVelge)
                throw new StemmeException($"For mange valg {idList.Count}, bare {KanVelge} er lov per delegat");

            if (_avgitStemme.Any(d => d.Id == delegat.Id))
            {
                List<Stemme> fjernes;
                if (Hemmelig)
                {
                    
                    if (revoteKey != null)
                    {
                        fjernes = _stemmer.Where(s => keyHasher.VerifyHash(s.RevoteKey, revoteKey)).ToList();
                    }
                    else
                    {
                        throw new StemmeException("Delegat har allerede stemmt");    
                    }
                }
                else
                {
                    fjernes = _stemmer.Where(s => s.DelegatId == delegat.Id).ToList();
                }

                if (!fjernes.Any())
                    throw new StemmeException("Det er registrert at delegaten har stemmt, men ingen stemmer ble funnnet. Noe er galt");
                
                _stemmer.RemoveAll(s=>  fjernes.Contains(s));
                fjernes.ForEach(s=> notificationManager.ForArrangement(Sak.ArrangementId).OnStemmeFjernet(new StemmeFjernetEvent(Id,s.Id)));
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

            stemmer.ForEach(s=> notificationManager.ForArrangement(Sak.ArrangementId).OnNyStemme(new NyStemmeEvent(Id,s)));
            return (stemmer, key);
        }

        public TEntity RegistrerStemme<TEntity>(IEnumerable<Guid> valgIder, Delegat delegat, string? gammelStemme, IKeyHasher keyHasher) where TEntity : class
        {
            throw new NotImplementedException();
        }
    }
}
