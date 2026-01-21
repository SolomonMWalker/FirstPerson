using Godot;
using System;
using FirstPerson.Scenes.Player;

public partial class StepHandlerComponent : Node
{
    [ExportCategory("References")] 
    [Export] public Player Player;

    [ExportCategory("Step Settings")]
    [Export] public float SurfaceThreshold { get; set; } = 0.3f;

    private string _stepStatus = "";

    public void HandleStepClimbing()
    {
        for (int i = 0; i < Player.GetSlideCollisionCount(); i++)
        {
            var collision = Player.GetSlideCollision(i);
            if (CheckCollisionNormal(collision))
            {
                GD.Print($"Vertical collision found {collision.GetNormal()}");
                break;
            }

            GD.Print("No vertical collision found");
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
        var resultNormal = (Vector3) result["normal"];
        if (result.Count > 0 && Mathf.Abs(resultNormal.Y) <= SurfaceThreshold)
        {
            _stepStatus = $"Raycast: vertical collision found {resultNormal}";
            return true;
        }

        _stepStatus = "No vertical collision detected";
        return false;
        
        //continuing from https://youtu.be/C5Je3eu5a2k?si=hYRgbWWyJjo9xRg4&t=121
    }
}
