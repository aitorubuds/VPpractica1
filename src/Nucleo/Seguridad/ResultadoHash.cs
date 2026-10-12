using Nucleo.Modelos;
using System;
using System.Collections.Generic;

namespace Nucleo.Seguridad;

public class ResultadoHash : IEquatable<ResultadoHash>
{
    public string Hash { get; init; } = string.Empty;
    public string Sal { get; init; } = string.Empty;
    public DateTime Creacion { get; init; } = DateTime.Now;

    public bool Equals(ResultadoHash? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Hash == other.Hash && Sal == other.Sal;
    }

    public override bool Equals(object? obj) => Equals(obj as ResultadoHash);

    public override int GetHashCode()
    {
        return HashCode.Combine(Hash, Sal);
    }

    public static bool operator ==(ResultadoHash? left, ResultadoHash? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ResultadoHash? left, ResultadoHash? right) => !(left == right);
}
