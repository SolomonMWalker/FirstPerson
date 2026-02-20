using Godot;
using System;
using FirstPerson.Helpers;

public partial class Grunt : Node3D
{
    [Export] public NavigationAgent3D NavigationAgent3D { get; set; }
    [Export] public CharacterBody3D CharacterBody3D { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public Node3D NavAgentMovementTargetNode { get; set; }

    [Export] public float Speed { get; set; } = 10f;

    private bool _walking, _ready;

    public override void _Ready()
    {
        base._Ready();
        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
        AnimationPlayer.Play("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleNavigation(delta);
        if (!_walking && CharacterBody3D.Velocity.LengthSquared() > 0)
        {
            AnimationPlayer.Play("idleToWalk");
            AnimationPlayer.Queue("walk");
            _walking = true;
        }
        else if (_walking && CharacterBody3D.Velocity.LengthSquared() == 0)
        {
            AnimationPlayer.Play("idle");
            _walking = false;
        }
    }

    private async void ActorSetup()
    {
        _ready = true;
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        NavigationAgent3D.TargetPosition = NavAgentMovementTargetNode.GlobalPosition;
    }
    
    protected virtual void HandleNavigation(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (!_ready || NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) == 0)
        {
            return;
        }
        
        NavigationAgent3D.TargetPosition = NavAgentMovementTargetNode.GlobalPosition;
        
        if (NavigationAgent3D.IsNavigationFinished())
        {
            CharacterBody3D.Velocity = Vector3.Zero;
            var stoppedDirection = (NavAgentMovementTargetNode.GlobalPosition - CharacterBody3D.GlobalPosition).Normalized();
            var targetRotation = Mathf.Atan2(stoppedDirection.X, stoppedDirection.Z);
            CharacterBody3D.Rotation = CharacterBody3D.Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
            CharacterBody3D.Velocity = HandleGravity(CharacterBody3D.Velocity, delta);
            CharacterBody3D.MoveAndSlide();
            return;
        }

        var nextPathPosition = NavigationAgent3D.GetNextPathPosition();
        var direction = (nextPathPosition - CharacterBody3D.GlobalPosition).Normalized();
        var tempVelocity = (direction * Speed) with { Y = NavigationAgent3D.Velocity.Y};
        CharacterBody3D.Velocity = tempVelocity;
        
        if (direction.Length() > 0.01f)
        {
            var targetRotation = Mathf.Atan2(direction.X, direction.Z);
            CharacterBody3D.Rotation = CharacterBody3D.Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
        }
        
        CharacterBody3D.Velocity = HandleGravity(CharacterBody3D.Velocity, delta);
        CharacterBody3D.MoveAndSlide();
    }

    private Vector3 HandleGravity(Vector3 velocity, double delta)
    {
        if (!CharacterBody3D.IsOnFloor())
        {
            velocity.Y -= 20 * (float)delta;
        }

        return velocity;
    }
}
