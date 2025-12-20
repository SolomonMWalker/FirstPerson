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

    public static Vector3 GetAxisRotationsToTarget(Node3D source, Vector3 target)
    {
        var transform = source.Transform.LookingAt(target);
        return transform.Basis.GetEuler();
    }
    
    public static void RotateForwardToTargetOnXAxis(Node3D node, Vector3 target, int? maxAngleInDeg = null)
    {
        var localTarget = node.ToLocal(target);
        var targetZy = new Vector2(localTarget.Z, localTarget.Y);
        var angle = Vector2.Zero.AngleToPoint(targetZy);
        GD.Print($"angle {Mathf.DegToRad(angle)}");
    }
    
    public static void RotateForwardToTargetOnYAxis(Node3D node, Vector3 target, int? maxAngleInDeg = null)
    {
        //forward is (0,0,-1)
        var sourceXz = new Vector2(node.GlobalPosition.X, node.GlobalPosition.Z);
        var targetXz = new Vector2(target.X, target.Z);
        var direction = sourceXz - targetXz;
        var angle = Mathf.Atan2(direction.X, direction.Y);
        angle = Mathf.LerpAngle(node.Rotation.Y, angle, 0.9f);
        if (maxAngleInDeg.HasValue)
        {
            var maxAngleInRads = Mathf.DegToRad(maxAngleInDeg.Value);
            angle = Mathf.Clamp(angle, -maxAngleInRads, maxAngleInRads);
        }
        node.Rotation = new Vector3(node.Rotation.X, angle, node.Rotation.Z);
    }
}