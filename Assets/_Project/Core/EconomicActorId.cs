using System;
namespace Signal.Core
{
    public readonly struct EconomicActorId : IEquatable<EconomicActorId>
    {
        private readonly string _value;
        public EconomicActorId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Economic actor ID must not be empty.", nameof(value));
            _value = value.Trim();
        }
        public string Value => _value ?? string.Empty;
        public bool IsValid => !string.IsNullOrWhiteSpace(_value);
        public static EconomicActorId Player => new EconomicActorId("player");
        public bool Equals(EconomicActorId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is EconomicActorId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value;
        public static bool operator ==(EconomicActorId left, EconomicActorId right) => left.Equals(right);
        public static bool operator !=(EconomicActorId left, EconomicActorId right) => !left.Equals(right);
    }
}
