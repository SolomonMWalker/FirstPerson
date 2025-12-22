using System.Collections.Generic;
using System.Linq;
using FirstPerson;
using FirstPerson.CustomTypes;
using FirstPerson.Helpers;
using Godot;

public abstract partial class Agent : HittableCharacterBody3D
{
    [Export] public int Health { get; protected set; } = 25;
    [Export] public int InitialStaggerHealth { get; protected set; } = 10;
    [Export] public int Speed { get; protected set; } = 3;
    [Export] public bool UseMoveToTargetFuzziness { get; protected set; }
    [Export] public float MoveToTargetFuzziness { get; protected set; } = 0.1f;
    [Export] public float TimeBeforeNextStagger { get; protected set; } = 4;
    [Export] public float TimeBeforeStaggerHealthRegen { get; protected set; } = 2.5f;
    [Export] public float StaggerRegenPercentPerSecond { get; protected set; } = 50;
    [Export] public float WeakpointHealthDamageMultiplier { get; protected set; } = 1.5f;
    [Export] public float WeakpointStaggerDamageMultiplier { get; protected set; } = 1.5f;
    [Export] public float MovementTargetAcquisitionPollTimeInSeconds { get; protected set; } = 0.15f;
    [Export] public float LineOfSightPollTimeInSeconds { get; protected set; } = 0.2f;
    [Export] public float StaggerTimeInSeconds { get; protected set; } = 2.0f;
    [Export] public int CoverSpotMaxTargetDistance { get; protected set; } = 40;
    [Export] public float DefaultTargetDistance { get; protected set; } = 0.5f;
    [Export] public int LineOfSightCheckRange { get; protected set; } = 250;
    [Export] public int PathStrayMaxDistance { get; protected set; } = 30;
    
    public Vector3 NavAgentMovementTarget
    {
        get => NavAgent.TargetPosition;
        protected set => NavAgent.TargetPosition = value;
    }

    protected HittableCharacterBody3D Target { get; set; }
    protected CoverSpotController CoverSpotController { get; set; }
    protected NavigationAgent3D NavAgent { get; set; }
    protected AnimationPlayer AnimationPlayer { get; set; }
    protected RayCast3D LineOfSightRayCast3D { get; set; }
    protected Area3D LineOfSightArea3D { get; set; }
    protected CoverSpot CurrentCoverSpot { get; set; }
    protected StaggerHealth StaggerHealth { get; set; }
    protected Poll MovementTargetAcquisitionPoll { get; set; }
    protected Poll LineOfSightPoll { get; set; }
    protected float CharacterRadius { get; set; } = 0.5f;
    protected float? CurrentFollowDistance;
    protected bool IsStaggered { get; set; }
    protected bool QueuedForDeath { get; set; }
    protected bool TargetInLineOfSight { get; set; }
    protected bool FreezeMotion { get; set; }
    protected List<Goal> AllowedGoals = [];
    protected Goal CurrentGoal { get; set; }
    protected List<AgentMovementState> MovementStates = [AgentMovementState.Still, AgentMovementState.DefaultMoving];
    protected AgentMovementState CurrentAgentMovementState { get; set; } = AgentMovementState.Still;
    
    public override void _Ready()
    {
        base._Ready();

        StaggerHealth = new StaggerHealth(InitialStaggerHealth, TimeBeforeStaggerHealthRegen, 
            StaggerRegenPercentPerSecond, TimeBeforeNextStagger);
        
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        CoverSpotController = GetNode<CoverSpotController>("../../CoverSpotController");

        LineOfSightRayCast3D = GetNode<RayCast3D>("LineOfSightRayCast3D");
        LineOfSightRayCast3D.Enabled = false;
        LineOfSightArea3D = GetNode<Area3D>("LineOfSightArea3D");
        
        MovementTargetAcquisitionPoll = new Poll(MovementTargetAcquisitionPollTimeInSeconds, Fuzzer.Fuzz(0f, 0.3f, false));
        LineOfSightPoll = new Poll(LineOfSightPollTimeInSeconds, Fuzzer.Fuzz(0f, 0.3f, false));
        
        //Nav agent https://docs.godotengine.org/en/stable/tutorials/navigation/navigation_introduction_3d.html#setup-for-3d-scene
        NavAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        NavAgent.PathHeightOffset = -1;
        // These values need to be adjusted for the actor's speed
        // and the navigation layout.
        NavAgent.PathMaxDistance = PathStrayMaxDistance;
        NavAgent.PathDesiredDistance = 0.5f;
        NavAgent.TargetDesiredDistance = DefaultTargetDistance;

        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
    }
    
    private async void ActorSetup()
    {
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        NavAgentMovementTarget = GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleRotation();
        CalculateIfTargetInLineOfSightWithPoll(delta);
        CalculateNavigation(delta);
        HandleNavigation();
        CalculateMovementState();
        StaggerHealth.CheckStaggerRegain(delta);
    }

    public override void Hit(HitParameters hitParameters)
    {
        base.Hit(hitParameters);
        var healthDamageMult = hitParameters.IsWeakspot ? WeakpointHealthDamageMultiplier : 1;
        var staggerDamageMult = hitParameters.IsWeakspot ? WeakpointStaggerDamageMultiplier : 1;
        DecreaseHealth( Mathf.RoundToInt(hitParameters.HealthDamage * healthDamageMult));
        DecreaseStaggerHealth(Mathf.RoundToInt(hitParameters.StaggerDamage * staggerDamageMult));
        if(!QueuedForDeath && !AnimationPlayer.IsPlaying()) AnimationPlayer.Play(
            hitParameters.IsWeakspot ? "WeakspotShot" : "Shot");
    }

    protected virtual bool IsMotionFrozen()
    {
        return FreezeMotion || IsStaggered;
    }

    protected virtual bool IsRotationFrozen()
    {
        return IsStaggered;
    }

    protected virtual bool IsActivityFrozen()
    {
        return IsStaggered;
    }

    protected virtual void SetGoal(Goal goal)
    {
        CurrentGoal = goal;
    }

    protected void ChangeMovementState(AgentMovementState agentMovementState)
    {
        if (!MovementStates.Contains(agentMovementState))
        {
            GD.PrintErr($"Movement state {agentMovementState} is not in the allowed agent movement states");
        }

        CurrentAgentMovementState = agentMovementState;
    }

    protected virtual void CalculateMovementState()
    {
        CurrentAgentMovementState = Velocity.IsZeroApprox() ? AgentMovementState.Still : AgentMovementState.DefaultMoving;
    }

    protected abstract void CalculateNavigation(double delta);

    protected virtual void MoveToTarget(double delta)
    {
        if (!MovementTargetAcquisitionPoll.IsPollPinged(delta)) return;
        SetNavigationToTarget(CurrentFollowDistance ?? 0 + CharacterRadius);
    }
    
    protected virtual void MoveToCover(double delta)
    {
        if (!MovementTargetAcquisitionPoll.IsPollPinged(delta)) return;
        if (IsMotionFrozen() || Target is null) return;
        if (!TargetInLineOfSight)
        {
            SetNavigationToTarget(CurrentFollowDistance);
            return;
        }
        var bestCoverSpot = CoverSpotController.GetViableCoverSpot(this, Target, CurrentCoverSpot);
        if (bestCoverSpot == null || bestCoverSpot.GlobalPosition.DistanceTo(Target.GlobalPosition) > CoverSpotMaxTargetDistance)
        {
            CurrentCoverSpot?.Unoccupy();
            CurrentCoverSpot = null;
            SetNavigationToTarget(CurrentFollowDistance); 
        }
        else
        {
            if (CurrentCoverSpot != bestCoverSpot)
            {
                CurrentCoverSpot?.Unoccupy();
                CurrentCoverSpot = bestCoverSpot;
                CurrentCoverSpot.Occupy(this);
            }

            SetNavigationToCoverSpot();
        }
    }

    protected virtual void HandleNavigation()
    {
        if (NavAgent.IsNavigationFinished() || IsMotionFrozen())
        {
            Velocity = Vector3.Zero;
            return;
        }

        var currentAgentPosition = GlobalTransform.Origin;
        var nextPathPosition = NavAgent.GetNextPathPosition();
        
        //don't overshoot, I think
        //if magnitude of this frame of velocity will overshoot target, just go directly to target
        //var direction = nextPathPosition - currentAgentPosition;
        var tempVelocity = currentAgentPosition.DirectionTo(nextPathPosition) * Speed;
        // if (direction.LengthSquared() < tempVelocity.LengthSquared())
        // {
        //     tempVelocity = direction;
        // }

        Velocity = Velocity.Lerp(tempVelocity, 0.95f);
        //Velocity = tempVelocity;
        MoveAndSlide();
    }

    protected virtual void DecreaseHealth(int amount)
    {
        Health -= amount;
        if (Health > 0) return;
        QueueFree();
        QueuedForDeath = true;
    }

    protected virtual void DecreaseStaggerHealth(int amount)
    {
        //GD.Print($"StaggerHealth is at {StaggerHealth.Amount}, decreasing by {amount}");
        if (StaggerHealth.IsStaggeredFromDecreaseStaggerHealth(amount))
        {
            AnimationPlayer.Play("Staggered");
        }
    }

    protected virtual void SetNavigationToTarget(float? distance = null)
    {
        if (Target is null) return;
        var point = HelperMethods.GetPointMetersFromTarget(Target.GlobalPosition, GlobalPosition,
            CurrentFollowDistance ?? 0);
        var target = !distance.HasValue || distance == 0 || !CurrentFollowDistance.HasValue 
            ? Target.GlobalPosition : point;
        target = UseMoveToTargetFuzziness ? Fuzzer.Fuzz(target, MoveToTargetFuzziness) : target;
        if (!NavAgentMovementTarget.Equals(target))
        {
            NavAgentMovementTarget = target;
        }
    }

    protected virtual void SetNavigationToCoverSpot()
    {
        if (!NavAgentMovementTarget.Equals(CurrentCoverSpot.GlobalPosition))
        {
            NavAgentMovementTarget = CurrentCoverSpot.GlobalPosition;
        }
    }

    protected virtual void LookAtMovementDirection()
    {
        if (!Velocity.IsZeroApprox())
        {
            var lookAtDirection = GlobalPosition + Velocity.Normalized();
            var rotVector = HelperMethods.GetAxisRotationsToTarget(this, lookAtDirection);
            Rotation = new Vector3(Rotation.X, rotVector.Y, Rotation.Z);
        }
        else
        {
            LookAtTarget();
        }
    }

    protected virtual void BeginStagger() => IsStaggered = true;

    protected virtual void EndStagger()
    {
        IsStaggered = false;
        StaggerHealth.EndStagger();
    }

    protected virtual void LookAtTarget()
    {
        if (Target is null) return;
        var rotVector = HelperMethods.GetAxisRotationsToTarget(this, Target.GlobalPosition);
        Rotation = new Vector3(Rotation.X, rotVector.Y, Rotation.Z);
    }

    protected virtual void HandleRotation()
    {
        if (IsRotationFrozen() || Target is null) return;
        var rotVector = HelperMethods.GetAxisRotationsToTarget(this, Target.GlobalPosition);
        Rotation = new Vector3(Rotation.X, rotVector.Y, Rotation.Z);
    }
    
    protected virtual void CalculateIfTargetInLineOfSightWithPoll(double delta)
    {
        if (!LineOfSightPoll.IsPollPinged(delta)) return;
        TargetInLineOfSight = CalcaulateIfTargetInLineOfSightWithArea3D() && CalculateIfTargetInLineOfSightWithRaycast();
    }

    protected virtual bool CalcaulateIfTargetInLineOfSightWithArea3D()
    {
        if (!LineOfSightArea3D.HasOverlappingBodies()) return false;
        var bodies = LineOfSightArea3D.GetOverlappingBodies();
        return bodies.Any(b => b is HittableCharacterBody3D hittable && hittable == Target);
    }
    
    protected virtual bool CalculateIfTargetInLineOfSightWithRaycast()
    {
        if (Target is null)
        {
            return false;
        }

        var ray = LineOfSightRayCast3D.Position.DirectionTo(ToLocal(Target.GlobalPosition));
        LineOfSightRayCast3D.TargetPosition = ray * LineOfSightCheckRange;
        LineOfSightRayCast3D.ForceRaycastUpdate();
        if (!LineOfSightRayCast3D.IsColliding())
        {
            return false;
        }
        var collided = LineOfSightRayCast3D.GetCollider();
        if (collided is HittableCharacterBody3D hittable)
        {
            return hittable == Target;
        }
        return false;
    }
}
