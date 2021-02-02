using System;

namespace Stemmesystem.Data
{
    public record Stemme
    {
        public Guid Id { get; private set; }
        public Guid ValgId { get; private set; }

        private Stemme() { }
        public Stemme(Guid valgId)
        {
            ValgId = valgId;
        }

        //public Valg Valg { get => valg ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Valg)); private set => valg = value; }
        public Delegat? Delegat { get; internal set; }
        public int? DelegatId { get; internal set; }

        internal string? RevoteKey { get; set; }
    }
}
