using Godot;
using System;
using FirstPerson.Scenes.Player;

public partial class StepHandlerComponent : Node
{
    [ExportCategory("References")] 
    [Export] public Player Player;

    [ExportCategory("Step Settings")]
    [Export] public float SurfaceThreshold { get; set; } = 0.3f;
    [Export] public float StepHeight { get; set; } = 0.5f;

    private const float MinStepHeight = 0.1f;
    private const float MinMovementLength = 0.1f;
    private const float MinDotValue = 0.5f;
    private string _stepStatus = "";

    public void HandleStepClimbing()
    {
        _stepStatus = "No vertical collision detected";
        
        for (int i = 0; i < Player.GetSlideCollisionCount(); i++)
        {
            var collision = Player.GetSlideCollision(i);
            if (IsVerticalSurface(collision))
            {
                var measuredHeight = MeasureStepHeight(collision);
                if (measuredHeight >= MinStepHeight && measuredHeight <= StepHeight && IsValidStepDirection(collision))
                {
                    Player.GlobalPosition = Player.GlobalPosition with { Y = Player.GlobalPosition.Y + measuredHeight };
                    Player.Velocity = Player.PreviousFrameVelocity;
                    Player.CameraController.SmoothStep(measuredHeight);
                    _stepStatus = $"Step found! Height: {measuredHeight}";
                }
                else
                {
                    _stepStatus = $"Step too high: {measuredHeight}";
                }
            }
        }
    }

    public bool CheckCollisionNormal(KinematicCollision3D collision)
    {
        var normal = collision.GetNormal();
        return !(Mathf.Abs(normal.Y) > SurfaceThreshold);
    }

    public bool IsVerticalSurface(KinematicCollision3D collision)
    {
        var normal = collision.GetNormal();
        if (Mathf.Abs(normal.Y) <= SurfaceThreshold)
        {
            //stepStatus = $"CollisionShape: Vertical collision found! {normal.ToString()}";
            return true;
        }

        return CheckCollisionSurface(collision);
    }

    public bool CheckCollisionSurface(KinematicCollision3D collision)
    {
        var spaceState = Player.GetWorld3D().DirectSpaceState;
        var collisionPoint = collision.GetPosition();

        var playerFeet = Player.BottomOfPlayer.GlobalPosition;
        collisionPoint.Y = playerFeet.Y;

        var query = PhysicsRayQueryParameters3D.Create(playerFeet, collisionPoint);
        query.CollisionMask = Player.CollisionMask;
        query.Exclude = [Player.GetRid()];

        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0&& result.TryGetValue("normal", out var normalVariant))
        {
            var resultNormal = (Vector3) normalVariant;
            _stepStatus = $"Raycast: vertical collision found {resultNormal}";
            return Mathf.Abs(resultNormal.Y) <= SurfaceThreshold;
        }

        _stepStatus = "No vertical collision detected";
        return false;
    }

    public float MeasureStepHeight(KinematicCollision3D collision)
    {
        var spaceState = Player.GetWorld3D().DirectSpaceState;
        var collisionPoint = collision.GetPosition();

        var playerFeet = Player.BottomOfPlayer.GlobalPosition;
        var playerHeadY = Player.GlobalPosition.Y + ((CapsuleShape3D)Player.StandingCollisionShape.Shape).Height / 2;

        var rayStart = new Vector3(collisionPoint.X, playerHeadY, collisionPoint.Z);
        var rayEnd = new Vector3(collisionPoint.X, playerFeet.Y, collisionPoint.Z);
        
        var query = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd);
        query.CollisionMask = Player.CollisionMask;
        query.Exclude = [Player.GetRid()];

        var result = spaceState.IntersectRay(query);
        if (result.Count > 0 && result.TryGetValue("position", out var positionVariant))
        {
            var position = (Vector3)positionVariant;
            return position.Y - playerFeet.Y;
        }
        return 0;
    }

    public bool IsValidStepDirection(KinematicCollision3D collision)
    {
        var collisionNormal = collision.GetNormal();
        var inputDir = Player.InputDirections;
        var movementDirection = Player.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y);
        if (movementDirection.Length() > MinMovementLength)
        {
            movementDirection = movementDirection.Normalized();
            var dotProduct = movementDirection.Dot(-collisionNormal);
            return dotProduct > MinDotValue;
        }
        return false;
    }
}
