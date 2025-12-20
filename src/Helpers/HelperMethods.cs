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
    
    public static void LookAtPositionOnlyX(Node3D node, Vector3 target)
    {
        var sourceYz = new Vector2(node.GlobalPosition.Y, node.GlobalPosition.Z);
        var targetYz = new Vector2(target.Y, target.Z);
        var direction = sourceYz - targetYz;
        node.Rotation = new Vector3(
            Mathf.LerpAngle(node.Rotation.X, Mathf.Atan2(direction.X, direction.Y), 0.9f),
            node.Rotation.Y,
            //Mathf.Atan2(direction.X, direction.Y),
            node.Rotation.Z);
    }
}