using FirstPerson;
using FirstPerson.Configuration;
using Godot;

public partial class Enemy : ShootableCharacterBody3D
{
    [Export] public int Health { get; protected set; } = 100;
    [Export] public int Speed { get; protected set; } = 10;
    [Export] public float NavigationPollTimeInSeconds { get; protected set; } = 0.5f;
    [Export] public int CoverSpotMaxTargetDistance { get; protected set; } = 40;
    [Export] public int TargetFollowDistance { get; protected set; } = 10;

    public enum MovementState
    {
        Still,
        DefaultMoving,
    }

    public enum Goal
    {
        MoveToCover,
        MoveToTarget,
        Patrol,
        Standby
    }
    
    public Vector3 NavAgentMovementTarget
    {
        get => NavAgent.TargetPosition;
        protected set => NavAgent.TargetPosition = value;
    }

    protected ShootableCharacterBody3D Target { get; set; }
    protected CoverSpotController CoverSpotController { get; set; }
    protected NavigationAgent3D NavAgent { get; set; }
    protected AnimationPlayer AnimationPlayer { get; set; }

    protected Vector3 InitialPosition { get; set; }
    protected CoverSpot CurrentCoverSpot { get; set; }
    protected double TimeSinceTargetCoverPoll { get; set; } = 5;
    protected bool QueuedForDeath { get; set; }
    protected bool FreezeMotion { get; set; }
    protected float DefaultTargetDistance { get; set; } = 0.5f;
    protected MovementState CurrentMovementState { get; set; } = MovementState.Still;
    protected Goal CurrentGoal { get; set; } = Goal.MoveToCover;
    
    public override void _Ready()
    {
        base._Ready();
        InitialPosition = GlobalPosition;

        Target = GetNode<Player>(Configuration.GetConfigValues().PlayerSceneTreePath);
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        CoverSpotController = GetNode<CoverSpotController>("../../CoverSpotController"); 
        
        //Nav agent https://docs.godotengine.org/en/stable/tutorials/navigation/navigation_introduction_3d.html#setup-for-3d-scene
        NavAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        NavAgent.PathHeightOffset = -1;
        // These values need to be adjusted for the actor's speed
        // and the navigation layout.
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
        CalculateNavigation(delta);
        HandleNavigation();
        HandleRotation();
        CalculateMovementState();
    }

    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        DecreaseHealth(shotParameters.Damage);
        if(!QueuedForDeath && !AnimationPlayer.IsPlaying()) AnimationPlayer.Play("shot");
    }

    public void SetGoal(Goal goal)
    {
        if (CurrentGoal == Goal.MoveToCover)
        {
            TimeSinceTargetCoverPoll = NavigationPollTimeInSeconds + 1;
        }
        CurrentGoal = goal;
    }

    public void CalculateMovementState() => 
        CurrentMovementState = Velocity.IsZeroApprox() ? MovementState.Still : MovementState.DefaultMoving;

    public void CalculateNavigation(double delta)
    {
        switch (CurrentGoal)
        {
            case Goal.MoveToCover:
                MoveToCover(delta);
                break;
            case Goal.MoveToTarget:
                MoveToTarget();
                break;
        }
    }

    public void MoveToTarget()
    {
        SetNavigationToTarget();
    }
    
    public void MoveToCover(double delta)
    {
        if (TimeSinceTargetCoverPoll > NavigationPollTimeInSeconds)
        {
            TimeSinceTargetCoverPoll = 0;
            if (FreezeMotion) return;
            var bestCoverSpot = CoverSpotController.GetViableCoverSpot(this, Target, CurrentCoverSpot);
            if (bestCoverSpot == null || bestCoverSpot.GlobalPosition.DistanceTo(Target.GlobalPosition) > CoverSpotMaxTargetDistance)
            {
                CurrentCoverSpot?.Unoccupy();
                CurrentCoverSpot = null;
                SetNavigationToTarget(); 
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
            TimeSinceTargetCoverPoll += delta;
        }
    }

    public virtual void HandleNavigation()
    {
        if (FreezeMotion) return;
        if (NavAgent.IsNavigationFinished())
        {
            Velocity = Vector3.Zero;
            return;
        }

        Vector3 currentAgentPosition = GlobalTransform.Origin;
        Vector3 nextPathPosition = NavAgent.GetNextPathPosition();

        Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * Speed;
        MoveAndSlide();
    }

    public void DecreaseHealth(int amount)
    {
        GD.Print($"Health is at {Health}, decreasing by {amount}");
        Health -= amount;
        if (Health > 0) return;
        GD.Print($"We gonna die");
        QueueFree();
        QueuedForDeath = true;
    }

    public void SetNavigationToTarget()
    {
        NavAgent.TargetDesiredDistance = TargetFollowDistance;
        NavAgentMovementTarget = Target.GlobalPosition;
    }

    public void SetNavigationToCoverSpot()
    {
        NavAgent.TargetDesiredDistance = DefaultTargetDistance;
        NavAgentMovementTarget = CurrentCoverSpot.GlobalPosition;
    }

    public void LookAtMovementDirection()
    {

        if (!Velocity.IsZeroApprox())
        {
            var lookAtDirection = GlobalPosition + Velocity.Normalized();
            lookAtDirection.Y = GlobalPosition.Y;
            //https://old.reddit.com/r/godot/comments/1k66joq/how_do_i_silence_this_engine_warning/
            if(!lookAtDirection.Cross(Vector3.Up).IsZeroApprox()) 
                LookAt(lookAtDirection, Vector3.Up);
        }
        else
        {
            LookAtTarget();
        }
    }

    public void LookAtTarget()
    {
        var lookAtDirection = Target.GlobalPosition;
        lookAtDirection.Y = GlobalPosition.Y;
        if(!lookAtDirection.Cross(Vector3.Up).IsZeroApprox()
           && !(lookAtDirection - GlobalPosition).IsZeroApprox())
            LookAt(lookAtDirection, Vector3.Up);
    }

    public virtual void HandleRotation()
    {
        if (CurrentMovementState is MovementState.Still)
        {
            LookAtTarget();
            return;
        }
        LookAtMovementDirection();
    }
}
