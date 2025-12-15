using System.Collections.Generic;
using System.Linq;
using FirstPerson;
using FirstPerson.Helpers;
using Godot;

public abstract partial class Agent : HittableCharacterBody3D
{
    [Export] public int Health { get; protected set; } = 100;
    [Export] public int Speed { get; protected set; } = 3;
    [Export] public float MovementTargetAcquisitionPollTimeInSeconds { get; protected set; } = 1.0f;
    [Export] public float LineOfSightPollTimeInSeconds { get; protected set; } = 3.0f;
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
    protected CoverSpot CurrentCoverSpot { get; set; }
    protected float CharacterRadius { get; set; } = 0.5f;
    protected double TimeSinceNavPoll { get; set; } = 5;
    protected double TimeSinceLineOfSightPoll { get; set; } = 5;
    protected bool QueuedForDeath { get; set; }
    protected bool TargetInLineOfSight { get; set; }
    protected bool FreezeMotion { get; set; }
    protected readonly List<bool> FreezeMotionBools = [];
    protected AgentMovementState CurrentAgentMovementState { get; set; } = AgentMovementState.Still;
    protected Goal CurrentGoal { get; set; }
    protected List<Goal> AllowedGoals = [];
    protected List<AgentMovementState> MovementStates = [AgentMovementState.Still, AgentMovementState.DefaultMoving];
    protected float? CurrentFollowDistance;
    
    public override void _Ready()
    {
        base._Ready();
        FreezeMotionBools.Add(FreezeMotion);
        
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        CoverSpotController = GetNode<CoverSpotController>("../../CoverSpotController"); 
        
        LineOfSightRayCast3D = GetNode<RayCast3D>("LineOfSightRayCast3D");
        LineOfSightRayCast3D.Enabled = false;
        
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
        CalculateIfTargetInLineOfSight(delta);
        CalculateNavigation(delta);
        HandleRotation();
        HandleNavigation(delta);
        CalculateMovementState();
    }

    public override void Hit(HitParameters hitParameters)
    {
        base.Hit(hitParameters);
        DecreaseHealth(hitParameters.Damage);
        if(!QueuedForDeath && !AnimationPlayer.IsPlaying()) AnimationPlayer.Play("shot");
    }

    protected virtual bool IsMotionFrozen()
    {
        return FreezeMotion;
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
        if (TimeSinceNavPoll > MovementTargetAcquisitionPollTimeInSeconds)
        {
            SetNavigationToTarget(CurrentFollowDistance ?? 0 + CharacterRadius);
        }
        else
        {
            TimeSinceNavPoll += delta;
        }
    }
    
    protected virtual void MoveToCover(double delta)
    {
        if (TimeSinceNavPoll > MovementTargetAcquisitionPollTimeInSeconds)
        {
            TimeSinceNavPoll = 0;
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
        else
        {
            TimeSinceNavPoll += delta;
        }
    }

    protected virtual void HandleNavigation(double delta)
    {
        if (NavAgent.IsNavigationFinished() || IsMotionFrozen())
        {
            Velocity = Vector3.Zero;
            return;
        }

        Vector3 currentAgentPosition = GlobalTransform.Origin;
        Vector3 nextPathPosition = NavAgent.GetNextPathPosition();
        
        //don't overshoot, I think
        //if magnitude of this frame of velocity will overshoot target, just go directly to target
        var direction = nextPathPosition - currentAgentPosition;
        var tempVelocity = currentAgentPosition.DirectionTo(nextPathPosition) * Speed;
        if (direction.LengthSquared() < tempVelocity.LengthSquared())
        {
            tempVelocity = direction;
        }

        //Velocity = Velocity.Lerp(tempVelocity, 0.99f);
        Velocity = tempVelocity;
        MoveAndSlide();
    }

    protected virtual void DecreaseHealth(int amount)
    {
        GD.Print($"Health is at {Health}, decreasing by {amount}");
        Health -= amount;
        if (Health > 0) return;
        GD.Print($"We gonna die");
        QueueFree();
        QueuedForDeath = true;
    }

    protected virtual void SetNavigationToTarget(float? distance = null)
    {
        if (Target is null) return;
        var point = HelperMethods.GetPointMetersFromTarget(Target.GlobalPosition, GlobalPosition,
            CurrentFollowDistance ?? 0);
        var target = !distance.HasValue || distance == 0 || !CurrentFollowDistance.HasValue 
            ? Target.GlobalPosition : point;
        
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
            LookAtPosition(lookAtDirection);
        }
        else
        {
            LookAtTarget();
        }
    }

    protected virtual void LookAtTarget()
    {
        if (Target is null) return;
        var lookAtDirection = Target.GlobalPosition;
        LookAtPosition(lookAtDirection);
    }

    protected virtual void LookAtPosition(Vector3 lookAt)
    {
        var sourceXZ = new Vector2(GlobalPosition.X, GlobalPosition.Z);
        var targetXZ = new Vector2(lookAt.X, lookAt.Z);
        var direction = sourceXZ - targetXZ;
        Rotation = new Vector3(Rotation.X,
            //Mathf.LerpAngle(Rotation.Y, Mathf.Atan2(direction.X, direction.Y), 0.99f),
            Mathf.Atan2(direction.X, direction.Y),
            Rotation.Z);
    }

    protected virtual void HandleRotation()
    {
        // if (CurrentAgentMovementState is AgentMovementState.Still)
        // {
        //     LookAtTarget();
        //     return;
        // }
        // LookAtMovementDirection();
        LookAtTarget();
    }
    
    protected virtual void CalculateIfTargetInLineOfSight(double delta)
    {
        if (TimeSinceLineOfSightPoll > LineOfSightPollTimeInSeconds)
        {
            if (Target is null)
            {
                TargetInLineOfSight = false;
                return;
            }

            var ray = LineOfSightRayCast3D.Position.DirectionTo(ToLocal(Target.GlobalPosition));
            LineOfSightRayCast3D.TargetPosition = ray * LineOfSightCheckRange;
            LineOfSightRayCast3D.ForceRaycastUpdate();
            if (!LineOfSightRayCast3D.IsColliding())
            {
                TargetInLineOfSight = true;
                return;
            }
            var collided = LineOfSightRayCast3D.GetCollider();
            if (collided == null)
            {
                TargetInLineOfSight = false;
                return;
            }
            TargetInLineOfSight = collided.GetInstanceId() == Target.GetInstanceId();
        }
        else
        {
            TimeSinceLineOfSightPoll += delta;
        }
    }
}
