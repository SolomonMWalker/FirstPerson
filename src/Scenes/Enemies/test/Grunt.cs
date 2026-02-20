using Godot;
using System;
using FirstPerson.Helpers;

public partial class Grunt : Node3D
{
    [Export] public NavigationAgent3D NavigationAgent3D { get; set; }
    [Export] public CharacterBody3D CharacterBody3D { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public Area3D CombatTriggerArea { get; set; }
    [Export] public Node3D NavAgentMovementTargetNode { get; set; }

    [Export] public float Speed { get; set; } = 10f;

    private bool _walking, _ready;

    public override void _Ready()
    {
        base._Ready();
        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
        AnimationPlayer.Play("idle");
        NavigationAgent3D.VelocityComputed += OnVelocityComputed;
    }

    private async void ActorSetup()
    {
        _ready = true;
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        if (NavAgentMovementTargetNode != null)
        {
            NavigationAgent3D.TargetPosition = NavAgentMovementTargetNode.GlobalPosition;
        }
    }

    public virtual void SetTarget(Node3D target) => NavAgentMovementTargetNode = target;

    public virtual void HandleJustGravity(double delta)
    {
        var velocityNoXz = CharacterBody3D.Velocity with { X = 0, Z = 0 };
        var currentVelocity = AddGravityToVelocity(velocityNoXz, delta);
        OnVelocityComputed(currentVelocity);
    }
    
    public virtual void HandleNavigation(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (!_ready || NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) == 0)
        {
            return;
        }

        if (NavAgentMovementTargetNode == null) return;
        
        NavigationAgent3D.TargetPosition = NavAgentMovementTargetNode.GlobalPosition;
        
        if (NavigationAgent3D.IsNavigationFinished())
        {
            var velocityNoXz = CharacterBody3D.Velocity with { X = 0, Z = 0 };
            var gravOnlyVelocity = AddGravityToVelocity(velocityNoXz, delta);
            var stoppedDirection = (NavAgentMovementTargetNode.GlobalPosition - CharacterBody3D.GlobalPosition).Normalized();
            var targetRotation = Mathf.Atan2(stoppedDirection.X, stoppedDirection.Z);
            CharacterBody3D.Rotation = CharacterBody3D.Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
            OnVelocityComputed(gravOnlyVelocity);
            return;
        }

        var nextPathPosition = NavigationAgent3D.GetNextPathPosition();
        var direction = (nextPathPosition - CharacterBody3D.GlobalPosition).Normalized();
        var currentVelocity = AddGravityToVelocity(direction * Speed, delta);
        
        if (direction.Length() > 0.01f)
        {
            var targetRotation = Mathf.Atan2(direction.X, direction.Z);
            CharacterBody3D.Rotation = CharacterBody3D.Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
        }

        if (NavigationAgent3D.AvoidanceEnabled)
        {
            NavigationAgent3D.Velocity = currentVelocity;
        }
        else
        {
            OnVelocityComputed(currentVelocity);
        }
    }
    
    private void OnVelocityComputed(Vector3 safeVelocity)
    {
        CharacterBody3D.Velocity = safeVelocity;
        CharacterBody3D.MoveAndSlide();
    }

    protected virtual Vector3 AddGravityToVelocity(Vector3 velocity, double delta)
    {
        float newYVelocity;
        if (!CharacterBody3D.IsOnFloor())
        {
            newYVelocity = velocity.Y - 20f * (float)delta;
        }
        else
        {
            newYVelocity = 0;
        }

        return velocity with { Y = newYVelocity };
    }
}
