using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Tender.Domain.ValueObjects;

public sealed class Deadline : IEquatable<Deadline>
{
    public DateTime Value { get; }

    private Deadline(DateTime value) => Value = value;

    public static Deadline From(DateTime value) =>
        value <= DateTime.UtcNow ? throw new ArgumentOutOfRangeException(nameof(value), "Deadline must be in the future.") : new Deadline(value);

    public bool Equals(Deadline? other) => other is not null && Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is Deadline d && Equals(d);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(Deadline left, Deadline right) => left.Equals(right);
    public static bool operator !=(Deadline left, Deadline right) => !left.Equals(right);
    public override string ToString() => Value.ToString("O");
}
