using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Tender.Domain.ValueObjects;

public sealed class Money : IEquatable<Money>
{
    public decimal Value { get; }

    private Money(decimal value) => Value = value;

    public static Money From(decimal value) =>
        value <= 0 ? throw new ArgumentOutOfRangeException(nameof(value), "Amount must be positive.") : new Money(value);

    public bool Equals(Money? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Money m && Equals(m);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(Money left, Money right) => left.Equals(right);
    public static bool operator !=(Money left, Money right) => !left.Equals(right);
    public override string ToString() => Value.ToString("F2");
}