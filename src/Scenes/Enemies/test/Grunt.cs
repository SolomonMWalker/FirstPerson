using Godot;
using System;
using FirstPerson.Helpers;

public partial class Grunt : Node3D
{
    [ExportCategory("References")]
    [Export] public Node3D NavAgentMovementTargetNode { get; set; }
    [Export] public NavigationAgent3D NavigationAgent3D { get; set; }
    [Export] public CharacterBody3D CharacterBody3D { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public Area3D CombatTriggerArea { get; set; }
    [Export] public Timer FireRateTimer { get; set; }
    
    [ExportCategory("Enemy Settings")]
    [Export] public float Speed { get; set; } = 10f;
    [Export] public float FireRatePauseInSeconds { get; set; } = 5f;

    [ExportCategory("Animation Settings")]
    [ExportGroup("Names")]
    [Export] public StringName IdleGunDown { get; set; } = "idleWithGunDown";
    [Export] public StringName WalkGunDown { get; set; } = "walkGunDown";
    [Export] public StringName IdleGunReady { get; set; } = "idleWithGunReady";
    [Export] public StringName WalkGunReady { get; set; } = "walkGunReady";
    [Export] public StringName IdleGunDownToWalkGunDown { get; set; } = "idleToWalkGunDown";
    [Export] public StringName IdleGunReadyToWalkGunReady { get; set; } = "idleToWalkGunReady";
    [Export] public StringName Aim { get; set; } = "aimGun";
    [Export] public StringName Fire { get; set; } = "fireGun";

    public BehaviorState behaviorState = BehaviorState.Idle;
    public bool readyToFire = false;
    public bool firing = false;
    
    private bool _ready;

    public enum BehaviorState
    {
        Idle,
        Following
    }

    public override void _Ready()
    {
        base._Ready();
        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
        FireRateTimer.WaitTime = FireRatePauseInSeconds;
        FireRateTimer.Timeout += () =>
        {
            GD.Print("Ready to fire");
            readyToFire = true;
        };
        AnimationPlayer.Play(IdleGunDown);
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

    public virtual void RotateToTarget()
    {
        if (NavAgentMovementTargetNode is null) return;
        var direction = (NavAgentMovementTargetNode.GlobalPosition - CharacterBody3D.GlobalPosition).Normalized();
        var targetRotation = Mathf.Atan2(direction.X, direction.Z);
        CharacterBody3D.Rotation = CharacterBody3D.Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
    }

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
