using FirstPerson.agents.AiComponents;
using FirstPerson.CustomTypes.StateMachine;
using Godot;
using FirstPerson.scenes.enemies.test;

public partial class Grunt : CombatAgent
{
    [ExportCategory("Enemy Settings")]
    [Export] public float FireRatePauseInSeconds { get; set; } = 2f;
    [Export] public float ShootRange { get; set; } = 50f;
    
    [ExportCategory("Patrol Points")]
    [Export] public Godot.Collections.Array<Node3D> PatrolPoints;

    [ExportCategory("Components")]
    [Export] public AgentFollowComponent AgentFollowComponent { get; set; }
    [Export] public AgentStopComponent AgentStopComponent { get; set; }
    [Export] public AgentPatrolComponent AgentPatrolComponent { get; set; }
    
    [ExportCategory("References")]
    [Export] public CustomAnimationTree CustomAnimationTree { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public RayCast3D ShootRaycast { get; set; }
    [Export] public FuzzyStartTimer FireRateTimer { get; set; }

    public bool readyToFire, firing, aimingOver;

    public override bool CanChangeState() => base.CanChangeState() && !firing;
    public override bool CanMove() => !firing && base.CanMove();
    public virtual void StopFiring() => firing = false;
    public virtual void StopAiming() => aimingOver = true;

    public override void SetLastDamageDirection(Vector3 sourceGlobalPosition, Vector3 collisionGlobalPoint)
    {
        base.SetLastDamageDirection(sourceGlobalPosition, collisionGlobalPoint);
        CustomAnimationTree.TrySetParam("blend_position", dirLastDamageXz);
        CustomAnimationTree.TrySetParam("request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }

    public override void _Ready()
    {
        base._Ready();

        FireRateTimer.SetStartTime(FireRatePauseInSeconds);
        FireRateTimer.Timeout += () =>
        {
            GD.Print("Ready to fire");
            readyToFire = true;
        };
        defaultCombatAi = AgentFollowComponent;
        
        if (PatrolPoints.Count > 0)
        {
            AgentPatrolComponent.PatrolPoints = PatrolPoints;
            defaultNoncombatAi = AgentPatrolComponent;
            CurrentNavComponent = AgentPatrolComponent;
            EnemyStateMachine.HandleChangeStateEvent(this, new ChangeStateEventArgs("PatrolState"));
        }
        else
        {
            defaultNoncombatAi = AgentStopComponent;
            CurrentNavComponent = AgentStopComponent;
            EnemyStateMachine.HandleChangeStateEvent(this, new ChangeStateEventArgs("IdleState"));
        }
    }

    public virtual void Aim()
    {
        freezeRotation = true;
        shootTargetRelativePosition = ShootRaycast.ToLocal(MovementTarget.GlobalPosition);
    }

    public virtual void Fire()
    {
        ShootRaycast.TargetPosition = shootTargetRelativePosition;
        ShootRaycast.ForceRaycastUpdate();
        if (ShootRaycast.IsColliding())
        {
            var collided = ShootRaycast.GetCollider();
            if (collided is Hitbox hitbox)
            {
                hitbox.Hit(BuildHitInformation(ShootRaycast.GetCollisionPoint()));
            }
        }
    }

    protected override HitInformation BuildHitInformation(Vector3 collisionGlobalPosition)
    {
        return new HitInformation(
            healthDamage: Damage,
            staggerDamage: StaggerDamage,
            sourceCombatTargetNode3D: this,
            sourceGlobalPosition: GlobalPosition,
            collisionGlobalPosition: collisionGlobalPosition,
            pitch: 1,
            roll: 1
        );
    }

    public override void StartRagdoll()
    {
        base.StartRagdoll();
        CustomAnimationTree.Active = false;
        AnimationPlayer.Stop();
        AnimationPlayer.QueueFree();
    }
}
