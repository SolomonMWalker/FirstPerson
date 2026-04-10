using FirstPerson.agents.AiComponents;
using FirstPerson.scenes.enemies.test;
using Godot;
using EncounterZone = FirstPerson.environment.utilities.EncounterZone;

namespace FirstPerson.agents.enemies.test;

public abstract partial class MovingAgent : CharacterBody3D
{
    [ExportCategory("Moving Agent Settings")]
    [Export] public float Speed { get; set; }
    [Export] public float MaxFallSpeed { get; set; } = 50f;
    
    [ExportCategory("Moving Agent References")]
    [Export] public Node3D MovementTarget { get; set; }
    [Export] public NavigationAgent3D NavigationAgent3D { get; set; }

    protected BaseAiNavComponent CurrentNavComponent { get; set; }
    protected Vector3 _moveToPoint;
    protected Rid? _navMapId;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        CurrentNavComponent.HandleNavigation(delta);
    }
    public void SetCurrentNavComponent(BaseAiNavComponent newNavComponent)
    {
        CurrentNavComponent = newNavComponent;
    }
    public abstract void SetLastDamageDirection(Vector3 sourceGlobalPosition, Vector3 collisionGlobalPoint);
    public abstract void PostSpawnInitialize(EncounterZone encounterZone);
    public abstract bool HasAffectedBone();
    public abstract void SetAffectedBone(PhysicalBone3D physicalBone3D);
    public abstract void HandleFalling(double delta);
    public abstract void RotateToTarget();
    public abstract bool CanMove();
    public abstract bool CanRotate();
    public abstract void RotateToGlobalPoint(Vector3 globalPoint);
    public abstract void OnVelocityComputed(Vector3 safeVelocity);

    //Auto-assume rotate to direction moving or rotate to target if not moving
    public virtual void MoveToPoint(double delta, Vector3? globalPoint, float speed)
    {
        _navMapId ??= NavigationAgent3D.GetNavigationMap();
        
        if (globalPoint.HasValue)
        {
            _moveToPoint = globalPoint.Value;
        }

        NavigationAgent3D.TargetPosition =  CanMove()
            ? _moveToPoint : GlobalPosition;
        NavigationAgent3D.TargetPosition = NavigationServer3D
            .MapGetClosestPoint(_navMapId.Value, NavigationAgent3D.TargetPosition);
        
        if (NavigationAgent3D.IsNavigationFinished())
        {
            RotateToTarget();
            
            if (NavigationAgent3D.AvoidanceEnabled)
            {
                NavigationAgent3D.Velocity = Vector3.Zero;
            }
            else
            {
                OnVelocityComputed(Vector3.Zero);
            }
            return;
        }
        
        var nextPathPosition = NavigationAgent3D.GetNextPathPosition();
        var direction = (nextPathPosition - GlobalPosition).Normalized();
        var currentVelocity = direction * speed;
        currentVelocity = currentVelocity with { Y = 0 };
        
        if (direction.Length() > 0.01f)
        {
            RotateToGlobalPoint(direction);
        }
        else
        {
            RotateToTarget();
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
}