using Godot;

public partial class Enemy : ShootableCharacterBody3D
{
    [Export] public int health = 10;
    [Export] public int speed = 10;
    [Export] public Vector3 targetPosition1;
    [Export] public Vector3 targetPosition2;
    
    public Vector3 MovementTarget
    {
        get => _navAgent.TargetPosition;
        set => _navAgent.TargetPosition = value;
    }

    private NavigationAgent3D _navAgent;
    private AnimationPlayer _animationPlayer;
    private bool _queuedForDeath;
    
    public override void _Ready()
    {
        base._Ready();
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        
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
        if (_navAgent.IsNavigationFinished())
        {
            MovementTarget = MovementTarget == targetPosition1 ? targetPosition2 : targetPosition1;
        }
        
        Vector3 currentAgentPosition = GlobalTransform.Origin;
        Vector3 nextPathPosition = _navAgent.GetNextPathPosition();

        Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * speed;
        MoveAndSlide();
    }

    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        DecreaseHealth(shotParameters.Damage);
        if(!_queuedForDeath && !_animationPlayer.IsPlaying()) _animationPlayer.Play("shot");
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
        MovementTarget = targetPosition1;
    }
}
