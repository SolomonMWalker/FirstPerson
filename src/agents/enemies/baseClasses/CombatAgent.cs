using Godot;
using System.Collections.Generic;
using System.Linq;
using FirstPerson.agents.enemies.test;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test;
using FirstPerson.scenes.enemies.test.states;
using EncounterZone = FirstPerson.environment.utilities.EncounterZone;

public partial class CombatAgent : MovingAgent
{
    [ExportCategory("CombatAgent Settings")]
    [Export]
    public int Health { get; set; } = 50;

    [Export] public int StaggerAmount { get; set; } = 50;
    [Export] public int Damage { get; set; } = 10;
    [Export] public int StaggerDamage { get; set; } = 10;
    [Export] public float StaggeredDamageReceivedMult { get; set; } = 1.0f;
    [Export] public float DefaultDamageReceivedMult { get; set; } = 1.0f;

    [ExportCategory("CombatAgent Components")]
    [Export]
    public HealthComponent HealthComponent { get; set; }

    [Export] public StaggerComponent StaggerComponent { get; set; }
    [Export] public Godot.Collections.Array<Hitbox> Hitboxes { get; set; }

    [ExportCategory("References")]
    [Export]
    public EncounterZone EncounterZone
    {
        get => _encounterZone;
        set
        {
            _encounterZone = value;
            if (_encounterZone is null) return;
            _encounterZone.OnCombatStart += target =>
            {
                MovementTarget = target;
                inCombat = true;
            };
        }
    } private EncounterZone _encounterZone;
    [Export] public CollisionShape3D CollisionShape3D { get; set; }
    [Export] public Skeleton3D Skeleton3D { get; set; }
    [Export] public PhysicalBoneSimulator3D PhysicalBoneSimulator3D { get; set; }
    [Export] public EnemyStateMachine EnemyStateMachine { get; set; }
    [Export] public Area3D CombatTriggerArea { get; set; }
    [Export] public RayCast3D FloorRaycast { get; set; }

    public bool Staggered
    {
        get => _staggered;
        set
        {
            _staggered = value;
            if (_staggered)
            {
                HealthComponent.DamageMult = StaggeredDamageReceivedMult;
            }
            else
            {
                HealthComponent.DamageMult = DefaultDamageReceivedMult;
            }
        }
    }

    private bool _staggered;
    public bool freezeRotation, ragdoll, dead, falling, inCombat;
    public Vector3 shootTargetRelativePosition;
    public PhysicalBone3D affectedBone;
    public Vector3 dirLastDamage; //vel of bones on ragdoll
    public Vector2 dirLastDamageXz; //determines impact animation blend
    public float previousFrameVelocityLengthSquared;

    protected float Gravity { get; } = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    protected bool _ready;
    protected List<PhysicalBone3D> _allPBones = [];
    protected List<CollisionShape3D> _boneCollisionShapes = [];

    public virtual bool CanChangeState() => !dead;
    public override bool CanMove() => !(dead || ragdoll || falling || Staggered);
    public override bool CanRotate() => !(freezeRotation || Staggered);
    public virtual bool IsFloorRaycastColliding() => FloorRaycast.IsColliding();
    public virtual void StopStagger() => Staggered = false;
    public virtual void SetTarget(Node3D target) => MovementTarget = target;
    public virtual void RaycastSnapToFloor()
    {
        if (IsFloorRaycastColliding()) ApplyFloorSnap();
    }
    public override void SetLastDamageDirection(Vector3 sourceGlobalPosition, Vector3 collisionGlobalPoint)
    {
        //rotated because player forward is -z, it works
        dirLastDamage = (sourceGlobalPosition - collisionGlobalPoint).Rotated(Vector3.Up, Mathf.DegToRad(180))
            .Normalized();
        var relativeDirLastDamage = ToLocal(sourceGlobalPosition) - ToLocal(collisionGlobalPoint);
        dirLastDamageXz = new Vector2(relativeDirLastDamage.X, relativeDirLastDamage.Z).Normalized();
    }

    public override void _Ready()
    {
        base._Ready();
        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
        
        HealthComponent.SetHealth(Health, true);
        StaggerComponent.SetStagger(StaggerAmount,true);
        
        NavigationAgent3D.VelocityComputed += OnVelocityComputed;

        if (_encounterZone is not null)
        {
            _encounterZone.OnCombatStart += target =>
            {
                MovementTarget = target;
                inCombat = true;
            };
        }

        foreach (var hitbox in Hitboxes)
        {
            hitbox.OnHitSetCombatTarget += target =>
            {
                MovementTarget = target;
                inCombat = true;
                if (_encounterZone is not null && !_encounterZone.Alerted && MovementTarget is not null)
                {
                    _encounterZone.AlertZone(MovementTarget);
                }
            };
        }
        
        HealthComponent.OnDeath += () =>
        {
            GD.Print("combat agent died!");
            GetTree().CreateTimer(5).Timeout += () =>
            {
                QueueFree();
            };
            dead = true;
            ragdoll = true;
        };

        StaggerComponent.OnStagger += () =>
        {
            Staggered = true;
        };
        
        _allPBones.AddRange(PhysicalBoneSimulator3D.GetChildren().OfType<PhysicalBone3D>());
        foreach (var pBone in _allPBones)
        {
            var cShape = (CollisionShape3D) pBone.GetChild(0);
            _boneCollisionShapes.Add(cShape);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        ApplyFloorSnap();
        previousFrameVelocityLengthSquared = Velocity.LengthSquared();
        
        if (!inCombat && CombatTriggerArea.HasOverlappingAreas())
        {
            var overlapArea = CombatTriggerArea.GetOverlappingAreas().First();
            if (overlapArea is Hitbox hitbox)
            {
                MovementTarget = hitbox.Parent;
                EnemyStateMachine.HandleChangeStateEvent(this, new ChangeStateEventArgs("InCombatState"));
                inCombat = true;
            }
        }
    }

    private async void ActorSetup()
    {
        _ready = true;
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        if (MovementTarget != null)
        {
            NavigationAgent3D.TargetPosition = MovementTarget.GlobalPosition;
        }
    }

    private HitInformation BuildHitInformation(Vector3 collisionGlobalPosition)
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

    public override void RotateToTarget()
    {
        if (MovementTarget is null || freezeRotation) return;
        var direction = (MovementTarget.GlobalPosition - GlobalPosition).Normalized();
        var targetRotation = Mathf.Atan2(direction.X, direction.Z);
        Rotation = Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
    }

    public override void RotateToGlobalPoint(Vector3 globalPoint)
    {
        if (freezeRotation) return;
        var targetRotation = Mathf.Atan2(globalPoint.X, globalPoint.Z);
        Rotation = Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
    }
    
    public override void OnVelocityComputed(Vector3 safeVelocity)
    {
        Velocity = safeVelocity;
        MoveAndSlide();
    }

    public virtual Vector3 AddGravityToVelocity(Vector3 velocity, double delta)
    {
        return new Vector3(velocity.X, velocity.Y - Gravity * (float)delta, velocity.Z);
    }

    public override void SetAffectedBone(PhysicalBone3D physicalBone3D)
    {
        affectedBone = physicalBone3D;
    }

    public override void HandleFalling(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) == 0)
        {
            return;
        }
        
        var currentVelocity = Velocity;
        currentVelocity = AddGravityToVelocity(currentVelocity, delta);

        NavigationAgent3D.TargetPosition = GlobalPosition + currentVelocity;
        
        if (NavigationAgent3D.AvoidanceEnabled)
        {
            NavigationAgent3D.Velocity = currentVelocity;
        }
        else
        {
            OnVelocityComputed(currentVelocity);
        }
    }

    private void SetBoneCollisionShapesDisabled(bool disabled) => 
        _boneCollisionShapes.ForEach(cs => cs.Disabled = disabled);

    public virtual void StartRagdoll()
    {
        SetBoneCollisionShapesDisabled(false);
        CollisionShape3D.Disabled = true;
        FloorRaycast.Enabled = false;
        PhysicalBoneSimulator3D.Active = true;
        PhysicalBoneSimulator3D.PhysicalBonesStartSimulation();
        if (affectedBone is not null)
        {
            GD.Print($"dirLastDamage {dirLastDamage} affectedBone {affectedBone.Name}");
            affectedBone.ApplyCentralImpulse(dirLastDamage * 20);
        }
        else
        {
            GD.Print("affected bone is null");
        }
    }

    public override void PostSpawnInitialize(EncounterZone encounterZone)
    {
        if (encounterZone is null) return;
        EncounterZone = encounterZone;
        if (!EncounterZone.TryGetTarget(out var target)) return;
        MovementTarget = target;
        inCombat = true;
    }

    public override bool HasAffectedBone()
    {
        return true;
    }
}
