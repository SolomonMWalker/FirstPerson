using Godot;

namespace FirstPerson.Helpers;

public static class HelperMethods
{
    public static Vector3 GetPointMetersFromTarget(Vector3 target, Vector3 source, float distance)
        => target + target.DirectionTo(source) * distance;
    public static void LookAtTargetInterpolated(Node3D source, Node3D target, float weight)
    {
        Transform3D xForm = source.Transform; // Your transform
        xForm = xForm.LookingAt(target.GlobalTransform.Origin, Vector3.Up); 
        source.Transform = source.Transform.InterpolateWith(xForm, weight);
    }
}