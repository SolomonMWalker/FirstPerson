using Godot;

namespace FirstPerson.Helpers;

public class HelperMethods
{
    public static Vector3 GetPointMetersFromTarget(Vector3 target, Vector3 source, float distance)
        => target + target.DirectionTo(source) * distance;
}