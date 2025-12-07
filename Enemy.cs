using FirstPerson;
using Godot;

public partial class Enemy : ShootableCharacterBody3D
{
    [Export] public int Health = 10;
    [Export] public int Speed = 10;
    [Export] public float CoverSpotPollTimeInSeconds = 0.5f;
    //Maximum distance before leaving the current target and going to the player
    [Export] public int CoverSpotMaxTargetDistance = 35;
    [Export] public int PlayerFollowDistance = 10;

    public enum BehaviorState
    {
        Default,
        //Patrolling,
        GoingToCover,
        AtCover
    }
    
    public Vector3 MovementTarget
    {
        get => _navAgent.TargetPosition;
        set => _navAgent.TargetPosition = value;
    }

    private Player _player;
    private CoverSpotController _coverSpotController;
    private NavigationAgent3D _navAgent;
    private AnimationPlayer _animationPlayer;

    private Vector3 _initialPosition;
    private CoverSpot _currentCoverSpot;
    private double _timeSincePlayerWasPolled = 5;
    private bool _queuedForDeath;
    private BehaviorState _currentBehaviorState = BehaviorState.Default;
    private float _defaultTargetDistance = 0.5f;
    
    public override void _Ready()
    {
        base._Ready();
        _initialPosition = GlobalPosition;

        _player = GetNode<Player>("/root/Test/Player");
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

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_timeSincePlayerWasPolled > CoverSpotPollTimeInSeconds)
        {
            _timeSincePlayerWasPolled = 0;
            var bestCoverSpot = _coverSpotController.GetViableCoverSpot(this, _player, _currentCoverSpot);
            if (bestCoverSpot == null || bestCoverSpot.GlobalPosition.DistanceTo(_player.GlobalPosition) > CoverSpotMaxTargetDistance)
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
        HandleNavigation();
        HandleRotation();
    }

    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        DecreaseHealth(shotParameters.Damage);
        if(!_queuedForDeath && !_animationPlayer.IsPlaying()) _animationPlayer.Play("shot");
    }
    
    private void HandleNavigation()
    {
        if (_navAgent.IsNavigationFinished())
        {
            _currentBehaviorState = BehaviorState.AtCover;
            return;
        }
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
    
    private async void ActorSetup()
    {
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        MovementTarget = GlobalPosition;
    }

    private void MoveToPlayer()
    {
        _navAgent.TargetDesiredDistance = PlayerFollowDistance;
        MovementTarget = _player.GlobalPosition;
    }

    private void MoveToCover()
    {
        _navAgent.TargetDesiredDistance = _defaultTargetDistance;
        MovementTarget = _currentCoverSpot.GlobalPosition;
    }

    private void LookAtMovementDirection()
    {
        //https://old.reddit.com/r/godot/comments/1k66joq/how_do_i_silence_this_engine_warning/
        if (Velocity != Vector3.Zero &&
            !(GlobalPosition + Velocity.Normalized()).Cross(Vector3.Up).IsZeroApprox() )
        {
            LookAt(GlobalPosition + Velocity.Normalized(), Vector3.Up);
        }
    }
    private void LookAtPlayer() => LookAt(_player.GlobalPosition);

    private void HandleRotation()
    {
        if (_currentBehaviorState is BehaviorState.AtCover || Velocity == Vector3.Zero)
        {
            LookAtPlayer();
            return;
        }
        LookAtMovementDirection();
    }
}
