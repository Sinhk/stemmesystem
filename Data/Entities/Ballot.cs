using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Models;
using Stemmesystem.Shared.Tools;
using DelegateEntity = Stemmesystem.Data.Entities.Delegate;

namespace Stemmesystem.Data.Entities
{
    [Table("Votering")]
    public class Ballot
    {
        private List<Choice> _choices = new();
        private List<Vote> _votes = new();
        private List<DelegateEntity> _votedDelegates = new();
        private Case? _case;

        public int Id { get; internal set; }
        [Column("Tittel")]
        public string Title { get; set; }
        [Column("Beskrivelse")]
        public string? Description { get; set; }
        [Column("Hemmelig")]
        public bool Secret { get; set; }
        [Column("Aktiv")]
        public bool Active { get; set; } = false;

        [Column("KanVelge")]
        public int MaxChoices { get; set; } = 1;

        [Column("StartTid")]
        public DateTime? StartTime { get; set; }
        [Column("SluttTid")]
        public DateTime? EndTime { get; set; }
        public IReadOnlyList<Choice> Choices => _choices;
        public List<Vote> Votes => _votes;
        public IReadOnlyList<DelegateEntity> VotedDelegates => _votedDelegates;

        public Case Case { get => _case ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Case)); set => _case = value; }
        [Column("SakId")]
        public int CaseId { get; internal set; }
        [Column("Lukket")]
        public bool Locked { get; set; }
        [Column("Publisert")]
        public bool Published { get; set; }

        public Ballot(string title, bool secret)
        {
            Title = title;
            Secret = secret;
        }

        public Ballot(string title, bool secret, params string[] choiceTexts) : this(title, secret)
        {
            var i = 0;
            foreach (var text in choiceTexts)
            {
                _choices.Add(new(text, i));
                i++;
            }
        }
        public Ballot(string title, bool secret, int? maxChoices, params string[] choiceTexts) : this(title, secret, choiceTexts)
        {
            MaxChoices = maxChoices.GetValueOrDefault(1);
        }

        public static Ballot SimpleVote(string title, bool secret = false) 
        {
            Ballot b = new(title, secret);
            b._choices.Add(new Choice("For"));
            b._choices.Add(new Choice("Mot"));
            return b;
        }

        public void Start()
        {
            if (Locked)
                throw new VotingException("Votering lukket, kan ikke startes igjen");
            Active = true;
            StartTime = DateTime.UtcNow;
        }

        [MemberNotNull(nameof(EndTime))]
        public void End(int delegatesPresent)
        {
            Active = false;
            EndTime = DateTime.UtcNow;
            DelegatesPresent = delegatesPresent;
        }

        /// <summary>
        /// Number of delegates present during the ballot
        /// </summary>
        [Column("DelegaterTilstede")]
        public int? DelegatesPresent { get; set; }

        public (List<Vote> votes, List<Vote>? removed) RegisterVote(IEnumerable<Guid> choiceIds, DelegateEntity delegateEntity, string delegateCode, IKeyHasher keyHasher)
        {
            List<Guid> idList = choiceIds.ToList();
            if(idList.Count > 1 && idList.Contains(Constants.BlankVote))
                throw new VotingException("Kan ikke ha flere valg i en blank stemme");
            
            if(idList.Count > MaxChoices)
                throw new VotingException($"For mange valg {idList.Count}, bare {MaxChoices} er lov per delegat");

            List<Vote>? removed = null;
            if (_votedDelegates.Any(d => d.Id == delegateEntity.Id))
            {
                removed = Secret ? _votes.Where(s => keyHasher.VerifyHash(s.VoteHash, delegateCode)).ToList() : _votes.Where(s => s.DelegateId == delegateEntity.Id).ToList();
                _votes.RemoveAll(s => removed.Contains(s));
            }

            List<Vote> votes = new(); 
            foreach (var choiceId in idList)
            {
                if (choiceId != Constants.BlankVote && _choices.All(v => v.Id != choiceId))
                    throw new VotingException("Ugyldig valg");
                Vote vote = new(choiceId);
                if (!Secret)
                    vote.DelegateId = delegateEntity.Id;
                vote.VoteHash = keyHasher.CreateHash(delegateCode);
                votes.Add(vote);
            }

            _votes.AddRange(votes);
            _votedDelegates.Add(delegateEntity);

            return (votes, removed);
        }

        public void Lock()
        {
            Locked = true;
        }
        
        public void Publish()
        {
            if (!Locked)
                throw new VotingException("Votering må lukkes før den kan publiseres");
            Published = true;
        }

        public Ballot Copy() =>
            new(Title, Secret)
            {
                Active = false
                , Description = Description
                , MaxChoices = MaxChoices
                , CaseId = CaseId
                , _choices = new List<Choice>(_choices.Select(v => new Choice(v.Name, v.SortId){Id = Guid.NewGuid()}))
            };
    }
}

