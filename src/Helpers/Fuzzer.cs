using System;
using Godot;

namespace FirstPerson.Helpers;

public class Fuzzer
{
    private static Random Random { get; } = new();

    public static bool Coinflip()
    {
        return Random.NextSingle() > 0.5;
    }

    public static int CoinflipPosOrNeg1() => Coinflip() ? 1 : -1;

    public static float Fuzz(float value, float range, bool signed = true)
    {
        var randomness = Random.NextSingle() * range * (signed ? CoinflipPosOrNeg1() : 1);
        return value + randomness;
    }

    public static Vector3 Fuzz(Vector3 value, float range, bool signed = true)
    {
        value.X = Fuzz(value.X, range, signed);
        value.Y = Fuzz(value.Y, range, signed);
        value.Z = Fuzz(value.Z, range, signed);
        return value;
    }
    
    public static Vector2 Fuzz(Vector2 value, float range, bool signed = true)
    {
        value.X = Fuzz(value.X, range, signed);
        value.Y = Fuzz(value.Y, range, signed);
        return value;
    }
}