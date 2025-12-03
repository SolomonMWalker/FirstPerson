using FirstPerson;
using Godot;

public partial class Enemy : ShootableCharacterBody3D
{
    [Export] public int health = 10;
    [Export] public int speed = 10;
    [Export] public float CoverSpotPollTimeInSeconds = 0.5f;
    
    public Vector3 MovementTarget
    {
        get => _navAgent.TargetPosition;
        set => _navAgent.TargetPosition = value;
    }
    
    private CoverSpotController _coverSpotController;
    private CoverSpot _currentCoverSpot;
    private NavigationAgent3D _navAgent;
    private AnimationPlayer _animationPlayer;
    private bool _queuedForDeath;
    private double _timeSincePlayerWasPolled;
    
    public override void _Ready()
    {
        base._Ready();
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _coverSpotController = GetNode<CoverSpotController>("../../CoverSpotController");
        
        //Nav agent https://docs.godotengine.org/en/stable/tutorials/navigation/navigation_introduction_3d.html#setup-for-3d-scene
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _navAgent.PathHeightOffset = -1;
        // These values need to be adjusted for the actor's speed
        // and the navigation layout.
        _navAgent.PathDesiredDistance = 0.5f;
        _navAgent.TargetDesiredDistance = 0.5f;

        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        //get player location   
        if (_timeSincePlayerWasPolled > CoverSpotPollTimeInSeconds)
        {
            _timeSincePlayerWasPolled = 0;
            var newCoverSpot = _coverSpotController.GetAndOccupyClosestUnoccupiedCoverSpot(this);
            if (newCoverSpot != null && newCoverSpot != _currentCoverSpot)
            {
                _currentCoverSpot = newCoverSpot;
                MovementTarget = _currentCoverSpot.GlobalPosition;
            }
        }
        else
        {
            _timeSincePlayerWasPolled += delta;
        }
        HandleNavigation();
    }

    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        DecreaseHealth(shotParameters.Damage);
        if(!_queuedForDeath && !_animationPlayer.IsPlaying()) _animationPlayer.Play("shot");
    }
    
    private void HandleNavigation()
    {
        if (_navAgent.IsNavigationFinished()) return;
        
        Vector3 currentAgentPosition = GlobalTransform.Origin;
        Vector3 nextPathPosition = _navAgent.GetNextPathPosition();

        Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * speed;
        MoveAndSlide();
    }

    private void DecreaseHealth(int amount)
    {
        health -= amount;
        if (health > 0) return;
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
}
