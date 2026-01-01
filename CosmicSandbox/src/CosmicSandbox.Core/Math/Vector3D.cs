using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CosmicSandbox.Core.Math;

/// <summary>
/// High-precision 3D vector for astronomical calculations
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vector3D : IEquatable<Vector3D>
{
    public double X;
    public double Y;
    public double Z;

    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3D Zero => new(0, 0, 0);
    public static Vector3D One => new(1, 1, 1);
    public static Vector3D UnitX => new(1, 0, 0);
    public static Vector3D UnitY => new(0, 1, 0);
    public static Vector3D UnitZ => new(0, 0, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Length() => System.Math.Sqrt(X * X + Y * Y + Z * Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double LengthSquared() => X * X + Y * Y + Z * Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Normalize()
    {
        double length = Length();
        return length > 0 ? this / length : Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Vector3D a, Vector3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Cross(Vector3D a, Vector3D b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Distance(Vector3D a, Vector3D b) => (a - b).Length();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DistanceSquared(Vector3D a, Vector3D b) => (a - b).LengthSquared();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(Vector3D a) => new(-a.X, -a.Y, -a.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(Vector3D v, double scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(double scalar, Vector3D v) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator /(Vector3D v, double scalar) => new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public bool Equals(Vector3D other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    public override bool Equals(object? obj) => obj is Vector3D other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public static bool operator ==(Vector3D left, Vector3D right) => left.Equals(right);

    public static bool operator !=(Vector3D left, Vector3D right) => !left.Equals(right);

    public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3})";
}
