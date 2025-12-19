using System;
using Godot;

namespace FirstPerson.Helpers;

public static class Fuzzer
{
    private static Random Random { get; } = new();
    private const float LowerRangeThreshold = 0.1f;

    public static bool Coinflip()
    {
        return Random.NextSingle() > 0.5;
    }

    public static int CoinflipPosOrNeg1() => Coinflip() ? 1 : -1;

    public static float Fuzz(float value, float range, bool signed = true)
    {
        if (range <= LowerRangeThreshold) return value;
        var randomness = Random.NextSingle() * range * (signed ? CoinflipPosOrNeg1() : 1);
        return value + randomness;
    }

    public static Vector3 Fuzz(Vector3 value, float range, bool signed = true)
    {
        if (range <= LowerRangeThreshold) return value;
        value.X = Fuzz(value.X, range, signed);
        value.Y = Fuzz(value.Y, range, signed);
        value.Z = Fuzz(value.Z, range, signed);
        return value;
    }
    
    public static Vector2 Fuzz(Vector2 value, float range, bool signed = true)
    {
        if (range <= LowerRangeThreshold) return value;
        value.X = Fuzz(value.X, range, signed);
        value.Y = Fuzz(value.Y, range, signed);
        return value;
    }
}