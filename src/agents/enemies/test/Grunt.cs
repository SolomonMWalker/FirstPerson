using Godot;
using System.Collections.Generic;
using System.Linq;
using FirstPerson.scenes.enemies.test;
using FirstPerson.scenes.enemies.test.states;

public partial class Grunt : CharacterBody3D
{
    [ExportCategory("Enemy Settings")]
    [Export] public int Health { get; set; } = 50;
    [Export] public int StaggerAmount { get; set; } = 50;
    [Export] public float Speed { get; set; } = 10f;
    [Export] public float MaxFallSpeed { get; set; } = 50f;
    [Export] public float FireRatePauseInSeconds { get; set; } = 2f;
    [Export] public float ShootRange { get; set; } = 50f;
    [Export] public int Damage { get; set; } = 10;
    [Export] public int StaggerDamage { get; set; } = 10;
    [Export] public float StaggeredDamageReceivedMult { get; set; } = 1.5f;
    [Export] public float DefaultDamageReceivedMult { get; set; } = 1.0f;

    [ExportCategory("Components")]
    [Export] public AgentFollowComponent AgentFollowComponent { get; set; }
    [Export] public AgentIdleComponent AgentIdleComponent { get; set; }
    [Export] public HealthComponent HealthComponent { get; set; }
    [Export] public StaggerComponent StaggerComponent { get; set; }
    
    [ExportCategory("References")]
    [Export] public Node3D NavAgentMovementTargetNode { get; set; }
    [Export] public NavigationAgent3D NavigationAgent3D { get; set; }
    [Export] public CollisionShape3D CollisionShape3D { get; set; }
    [Export] public Skeleton3D Skeleton3D { get; set; }
    [Export] public CustomAnimationTree CustomAnimationTree { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public PhysicalBoneSimulator3D PhysicalBoneSimulator3D { get; set; }
    [Export] public EnemyStateMachine EnemyStateMachine { get; set; }
    [Export] public Area3D CombatTriggerArea { get; set; }
    [Export] public RayCast3D ShootRaycast { get; set; }
    [Export] public RayCast3D FloorRaycast { get; set; }
    [Export] public Timer FireRateTimer { get; set; }

    [ExportCategory("Animation Settings")]
    [ExportGroup("Names")]
    [Export] public StringName IdleGunDownAnimation { get; set; } = "idleWithGunDown";
    [Export] public StringName WalkGunDownAnimation { get; set; } = "walkGunDown";
    [Export] public StringName IdleGunReadyAnimation { get; set; } = "idleWithGunReady";
    [Export] public StringName WalkGunReadyAnimation { get; set; } = "walkGunReady";
    [Export] public StringName IdleGunDownToWalkGunDownAnimation { get; set; } = "idleToWalkGunDown";
    [Export] public StringName IdleGunReadyToWalkGunReadyAnimation { get; set; } = "idleToWalkGunReady";
    [Export] public StringName Falling { get; set; } = "falling";
    [Export] public StringName IdleToFalling { get; set; } = "idleToFalling";
    [Export] public StringName FallingToIdle { get; set; } = "fallingToIdle";
    [Export] public StringName AimAnimation { get; set; } = "Edited/editedAimGun";
    [Export] public StringName FireAnimation { get; set; } = "Edited/editedFireGun";

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
    public bool readyToFire, firing, freezeRotation, ragdoll, dead, falling, aimingOver;
    public Vector3 shootTargetRelativePosition;
    public PhysicalBone3D affectedBone;
    public Vector3 dirLastDamage; //vel of bones on ragdoll
    public Vector2 dirLastDamageXz; //determines impact animation blend
    public float previousFrameVelocityLengthSquared;
    
    private float Gravity { get; } = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    private bool _ready, _staggered;
    private List<PhysicalBone3D> _allPBones = [];
    private List<CollisionShape3D> _boneCollisionShapes = [];
    
    public virtual bool CanMove() => !(dead || ragdoll || firing || falling || Staggered);
    public virtual bool CanRotate() => !(freezeRotation || Staggered);
    public virtual bool IsFloorRaycastColliding() => FloorRaycast.IsColliding();
    public virtual void StopFiring() => firing = false;
    public virtual void StopAiming() => aimingOver = true;
    public virtual void StopStagger() => Staggered = false;
    public virtual void SetTarget(Node3D target) => NavAgentMovementTargetNode = target;
    public virtual void RaycastSnapToFloor()
    {
        if (IsFloorRaycastColliding()) ApplyFloorSnap();
    }
    public void SetLastDamageDirection(Vector3 sourceGlobalPosition, Vector3 collisionGlobalPoint)
    {
        //rotated because player forward is -z, it works
        dirLastDamage = (sourceGlobalPosition - collisionGlobalPoint).Rotated(Vector3.Up, Mathf.DegToRad(180))
            .Normalized();
        var relativeDirLastDamage = ToLocal(sourceGlobalPosition) - ToLocal(collisionGlobalPoint);
        dirLastDamageXz = new Vector2(relativeDirLastDamage.X, relativeDirLastDamage.Z).Normalized();
        CustomAnimationTree.TrySetParam("impact", dirLastDamageXz);
        CustomAnimationTree.TrySetParam("impactOneShot", 1);
    }

    public override void _Ready()
    {
        base._Ready();
        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
        
        HealthComponent.SetHealth(Health, true);
        StaggerComponent.SetStagger(StaggerAmount,true);
        
        FireRateTimer.WaitTime = FireRatePauseInSeconds;
        FireRateTimer.Timeout += () =>
        {
            GD.Print("Ready to fire");
            readyToFire = true;
        };
        
        AnimationPlayer.Play(IdleGunDownAnimation);
        
        NavigationAgent3D.VelocityComputed += OnVelocityComputed;
        HealthComponent.OnDeath += () =>
        {
            GD.Print("grunt died!");
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

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsKeyLabelPressed(Key.G))
        {
            HealthComponent.Kill();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        ApplyFloorSnap();
        previousFrameVelocityLengthSquared = Velocity.LengthSquared();
    }

    private async void ActorSetup()
    {
        _ready = true;
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        if (NavAgentMovementTargetNode != null)
        {
            NavigationAgent3D.TargetPosition = NavAgentMovementTargetNode.GlobalPosition;
        }
    }

    public virtual void Aim()
    {
        freezeRotation = true;
        shootTargetRelativePosition = ShootRaycast.ToLocal(NavAgentMovementTargetNode.GlobalPosition);
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

    private HitInformation BuildHitInformation(Vector3 collisionGlobalPosition)
    {
        return new HitInformation(
            healthDamage: Damage,
            staggerDamage: StaggerDamage,
            sourceGlobalPosition: GlobalPosition,
            collisionGlobalPosition: collisionGlobalPosition,
            pitch: 1,
            roll: 1
        );
    }

    public virtual void RotateToTarget()
    {
        if (NavAgentMovementTargetNode is null || freezeRotation) return;
        var direction = (NavAgentMovementTargetNode.GlobalPosition - GlobalPosition).Normalized();
        var targetRotation = Mathf.Atan2(direction.X, direction.Z);
        Rotation = Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
    }

    public virtual void RotateToGlobalPoint(Vector3 globalPoint)
    {
        if (freezeRotation) return;
        var targetRotation = Mathf.Atan2(globalPoint.X, globalPoint.Z);
        Rotation = Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
    }
    
    public void OnVelocityComputed(Vector3 safeVelocity)
    {
        Velocity = safeVelocity;
        MoveAndSlide();
    }

    public virtual Vector3 AddGravityToVelocity(Vector3 velocity, double delta)
    {
        return new Vector3(velocity.X, velocity.Y - Gravity * (float)delta, velocity.Z);
    }

    public virtual void HandleFalling(double delta)
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

    public void StartRagdoll()
    {
        SetBoneCollisionShapesDisabled(false);
        FloorRaycast.Enabled = false;
        PhysicalBoneSimulator3D.Active = true;
        PhysicalBoneSimulator3D.PhysicalBonesStartSimulation();
        if (affectedBone is not null)
        {
            GD.Print($"dirLastDamage {dirLastDamage} affectedBone {affectedBone.Name}");
            affectedBone.LinearVelocity = dirLastDamage * 20f;
        }
        else
        {
            GD.Print("affected bone is null");
        }
    }


}
