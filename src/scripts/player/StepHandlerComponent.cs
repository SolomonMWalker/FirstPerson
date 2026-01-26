using Godot;
using System;
using FirstPerson.Scenes.Player;

public partial class StepHandlerComponent : Node
{
    [ExportCategory("References")] 
    [Export] public PlayerController PlayerController;

    [ExportCategory("Step Settings")]
    [Export] public float SurfaceThreshold { get; set; } = 0.3f;
    [Export] public float StepHeight { get; set; } = 0.5f;

    private const float HeightBuffer = 0.05f;
    private const float MinStepHeight = 0.1f;
    private const float MinMovementLength = 0.1f;
    private const float MinDotValue = 0.5f;
    private string _stepStatus = "";

    public void HandleStepClimbing()
    {
        _stepStatus = "No vertical collision detected";
        
        for (int i = 0; i < PlayerController.GetSlideCollisionCount(); i++)
        {
            var collision = PlayerController.GetSlideCollision(i);
            if (IsVerticalSurface(collision))
            {
                var measuredHeight = MeasureStepHeight(collision);
                if (measuredHeight > MinStepHeight && measuredHeight <= StepHeight && IsValidStepDirection(collision))
                {
                    PlayerController.GlobalPosition = PlayerController.GlobalPosition 
                        with { Y = PlayerController.GlobalPosition.Y + measuredHeight};
                    PlayerController.Velocity = PlayerController.PreviousFrameVelocity;
                    PlayerController.CameraController.SmoothStep(measuredHeight);
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
        var spaceState = PlayerController.GetWorld3D().DirectSpaceState;
        var collisionPoint = collision.GetPosition();

        var playerFeet = PlayerController.BottomOfPlayer.GlobalPosition;
        playerFeet.Y += HeightBuffer;
        collisionPoint.Y = playerFeet.Y;

        var query = PhysicsRayQueryParameters3D.Create(playerFeet, collisionPoint);
        query.CollisionMask = PlayerController.CollisionMask;
        query.Exclude = [PlayerController.GetRid()];

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
        var spaceState = PlayerController.GetWorld3D().DirectSpaceState;
        var collisionPoint = collision.GetPosition();

        var playerFeet = PlayerController.BottomOfPlayer.GlobalPosition;
        playerFeet.Y += HeightBuffer;
        var shape = (CapsuleShape3D)PlayerController.StandingCollisionShape.Shape;
        var playerCollisionShapeHeightOffset = shape.Height / 2;

        var playerHeadY = PlayerController.GlobalPosition.Y + playerCollisionShapeHeightOffset;

        var rayStart = new Vector3(collisionPoint.X, playerHeadY, collisionPoint.Z);
        var rayEnd = new Vector3(collisionPoint.X, playerFeet.Y, collisionPoint.Z);
        
        var query = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd);
        query.CollisionMask = PlayerController.CollisionMask;
        query.Exclude = [PlayerController.GetRid()];

        var result = spaceState.IntersectRay(query);
        if (result.Count > 0 && result.TryGetValue("position", out var resultPositionVariant))
        {
            var resultPosition = (Vector3) resultPositionVariant;
            return resultPosition.Y - playerFeet.Y;
        }
        return 0;
    }

    public bool IsValidStepDirection(KinematicCollision3D collision)
    {
        var collisionNormal = collision.GetNormal();
        var inputDir = PlayerController.InputDirections;
        var movementDirection = PlayerController.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y);
        if (movementDirection.Length() > MinMovementLength)
        {
            movementDirection = movementDirection.Normalized();
            var dotProduct = movementDirection.Dot(-collisionNormal);
            return dotProduct > MinDotValue;
        }
        return false;
    }
}
