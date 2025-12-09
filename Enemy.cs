using FirstPerson;
using Godot;

public partial class Enemy : ShootableCharacterBody3D
{
    [Export] public int Health = 100;
    [Export] public int Speed = 10;
    [Export] public float CoverSpotPollTimeInSeconds = 0.5f;
    [Export] public int CoverSpotMaxTargetDistance = 40;
    [Export] public int PlayerFollowDistance = 10;

    public enum BehaviorState
    {
        Default,
        //Patrolling,
        ChasingTarget,
        GoingToCover,
        AtCover
    }
    
    public Vector3 MovementTarget
    {
        get => _navAgent.TargetPosition;
        set => _navAgent.TargetPosition = value;
    }

    protected ShootableCharacterBody3D _target;
    protected CoverSpotController _coverSpotController;
    protected NavigationAgent3D _navAgent;
    protected AnimationPlayer _animationPlayer;

    protected Vector3 _initialPosition;
    protected CoverSpot _currentCoverSpot;
    protected double _timeSincePlayerWasPolled = 5;
    protected bool _queuedForDeath;
    protected bool _freezeMotion;
    protected BehaviorState _currentBehaviorState = BehaviorState.Default;
    protected float _defaultTargetDistance = 0.5f;
    
    public override void _Ready()
    {
        base._Ready();
        _initialPosition = GlobalPosition;

        _target = GetNode<Player>("/root/Test/Player");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _coverSpotController = GetNode<CoverSpotController>("../../CoverSpotController"); 
        
        //Nav agent https://docs.godotengine.org/en/stable/tutorials/navigation/navigation_introduction_3d.html#setup-for-3d-scene
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _navAgent.PathHeightOffset = -1;
        // These values need to be adjusted for the actor's speed
        // and the navigation layout.
        _navAgent.PathDesiredDistance = 0.5f;
        _navAgent.TargetDesiredDistance = _defaultTargetDistance;

        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
        _currentBehaviorState = BehaviorState.GoingToCover;
    }
    
    private async void ActorSetup()
    {
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        MovementTarget = GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        CalculateNavigation(delta);
        HandleNavigation();
        HandleRotation();
    }

    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        DecreaseHealth(shotParameters.Damage);
        if(!_queuedForDeath && !_animationPlayer.IsPlaying()) _animationPlayer.Play("shot");
    }

    private void CalculateNavigation(double delta)
    {
        if (_timeSincePlayerWasPolled > CoverSpotPollTimeInSeconds)
        {
            _timeSincePlayerWasPolled = 0;
            if (_freezeMotion) return;
            var bestCoverSpot = _coverSpotController.GetViableCoverSpot(this, _target, _currentCoverSpot);
            if (bestCoverSpot == null || bestCoverSpot.GlobalPosition.DistanceTo(_target.GlobalPosition) > CoverSpotMaxTargetDistance)
            {
                _currentCoverSpot?.Unoccupy();
                _currentCoverSpot = null;
                MoveToPlayer(); 
            }
            else
            {
                if (_currentCoverSpot != bestCoverSpot)
                {
                    _currentCoverSpot?.Unoccupy();
                    _currentCoverSpot = bestCoverSpot;
                    _currentCoverSpot.Occupy(this);
                }
                MoveToCover();
            }
        }
        else
        {
            _timeSincePlayerWasPolled += delta;
        }
    }
    
    protected virtual void HandleNavigation()
    {
        if (_freezeMotion) return;
        if (_navAgent.IsNavigationFinished())
        {
            _currentBehaviorState = _currentCoverSpot != null ? BehaviorState.AtCover : BehaviorState.ChasingTarget;
            return;
        }
        _currentBehaviorState = _currentCoverSpot != null ? BehaviorState.GoingToCover : BehaviorState.ChasingTarget;
        _currentBehaviorState = BehaviorState.GoingToCover;
        
        Vector3 currentAgentPosition = GlobalTransform.Origin;
        Vector3 nextPathPosition = _navAgent.GetNextPathPosition();

        Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * Speed;
        MoveAndSlide();
    }

    private void DecreaseHealth(int amount)
    {
        Health -= amount;
        if (Health > 0) return;
        QueueFree();
        _queuedForDeath = true;
    }

    private void MoveToPlayer()
    {
        _navAgent.TargetDesiredDistance = PlayerFollowDistance;
        MovementTarget = _target.GlobalPosition;
    }

    private void MoveToCover()
    {
        _navAgent.TargetDesiredDistance = _defaultTargetDistance;
        MovementTarget = _currentCoverSpot.GlobalPosition;
    }

    protected void LookAtMovementDirection()
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

    protected void LookAtTarget()
    {
        var lookAtDirection = _target.GlobalPosition;
        lookAtDirection.Y = GlobalPosition.Y;
        if(!lookAtDirection.Cross(Vector3.Up).IsZeroApprox()
           && !(lookAtDirection - GlobalPosition).IsZeroApprox())
            LookAt(lookAtDirection, Vector3.Up);
    }

    protected virtual void HandleRotation()
    {
        if (_currentBehaviorState is BehaviorState.AtCover)
        {
            LookAtTarget();
            return;
        }
        LookAtMovementDirection();
    }
}
