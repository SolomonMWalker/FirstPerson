using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FirstPerson.Helpers;
using FirstPerson.scenes.enemies.test;

public partial class Grunt : Node3D
{
    [ExportCategory("Enemy Settings")]
    [Export] public int Health { get; set; } = 50;
    [Export] public float Speed { get; set; } = 10f;
    [Export] public float FireRatePauseInSeconds { get; set; } = 5f;
    [Export] public float ShootRange { get; set; } = 50f;
    [Export] public int Damage { get; set; } = 10;
    
    [ExportCategory("Components")]
    [Export] public AgentFollowComponent AgentFollowComponent { get; set; }
    [Export] public AgentIdleComponent AgentIdleComponent { get; set; }
    [Export] public HealthComponent HealthComponent { get; set; }
    
    [ExportCategory("References")]
    [Export] public Node3D NavAgentMovementTargetNode { get; set; }
    [Export] public NavigationAgent3D NavigationAgent3D { get; set; }
    [Export] public CharacterBody3D CharacterBody3D { get; set; }
    [Export] public CollisionShape3D CollisionShape3D { get; set; }
    [Export] public Skeleton3D Skeleton3D { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public PhysicalBoneSimulator3D PhysicalBoneSimulator3D { get; set; }
    [Export] public Area3D CombatTriggerArea { get; set; }
    [Export] public RayCast3D ShootRaycast { get; set; }
    [Export] public Timer FireRateTimer { get; set; }

    [ExportCategory("Animation Settings")]
    [ExportGroup("Names")]
    [Export] public StringName IdleGunDownAnimation { get; set; } = "idleWithGunDown";
    [Export] public StringName WalkGunDownAnimation { get; set; } = "walkGunDown";
    [Export] public StringName IdleGunReadyAnimation { get; set; } = "idleWithGunReady";
    [Export] public StringName WalkGunReadyAnimation { get; set; } = "walkGunReady";
    [Export] public StringName IdleGunDownToWalkGunDownAnimation { get; set; } = "idleToWalkGunDown";
    [Export] public StringName IdleGunReadyToWalkGunReadyAnimation { get; set; } = "idleToWalkGunReady";
    [Export] public StringName AimAnimation { get; set; } = "Edited/editedAimGun";
    [Export] public StringName FireAnimation { get; set; } = "Edited/editedFireGun";

    public BehaviorState behaviorState = BehaviorState.Idle;
    public bool readyToFire, firing, freezeRotation, ragdoll, dead;
    public Vector3 shootTargetRelativePosition;
    public PhysicalBone3D affectedBone;
    public Vector3 dirLastDamage;
    
    private bool _ready;
    private List<PhysicalBone3D> _allPBones = [];
    private List<CollisionShape3D> _boneCollisionShapes = [];

    public enum BehaviorState
    {
        Idle,
        Following
    }

    public override void _Ready()
    {
        base._Ready();
        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
        
        HealthComponent.SetHealth(Health, true);
        
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
            GetTree().CreateTimer(5).Timeout += QueueFree;
            dead = true;
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
            var collided = (Node) ShootRaycast.GetCollider();
            GD.Print($"hit {collided.Name}");
            if (collided is Hitbox hitbox)
            {
                GD.Print("hit a hitbox!");
                var hitInfo = new HitInformation(healthDamage: Damage, staggerDamage: null,
                    sourceGlobalPosition: CharacterBody3D.GlobalPosition,
                    collisionGlobalPosition: ShootRaycast.GetCollisionPoint());
                hitbox.Hit(hitInfo);
            }
            else
            {
                GD.Print("dit NOT hit a hitbox!");
            }
        }
    }

    public virtual void SetTarget(Node3D target) => NavAgentMovementTargetNode = target;

    public virtual void RotateToTarget()
    {
        if (NavAgentMovementTargetNode is null || freezeRotation) return;
        var direction = (NavAgentMovementTargetNode.GlobalPosition - CharacterBody3D.GlobalPosition).Normalized();
        var targetRotation = Mathf.Atan2(direction.X, direction.Z);
        CharacterBody3D.Rotation = CharacterBody3D.Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
    }

    public virtual void RotateToGlobalPoint(Vector3 globalPoint)
    {
        var targetRotation = Mathf.Atan2(globalPoint.X, globalPoint.Z);
        CharacterBody3D.Rotation = CharacterBody3D.Rotation with { Y = targetRotation + Mathf.DegToRad(180) };
    }
    
    public void OnVelocityComputed(Vector3 safeVelocity)
    {
        CharacterBody3D.Velocity = safeVelocity;
        CharacterBody3D.MoveAndSlide();
    }

    public virtual Vector3 AddGravityToVelocity(Vector3 velocity, double delta)
    {
        float newYVelocity;
        if (!CharacterBody3D.IsOnFloor())
        {
            newYVelocity = velocity.Y - 100f * (float)delta;
        }
        else
        {
            newYVelocity = 0;
        }

        return velocity with { Y = newYVelocity };
    }

    public void SetLastDamageDirection(Vector3 sourceGlobalPosition, Vector3 collisionPoint)
    {
        dirLastDamage = (collisionPoint - sourceGlobalPosition).Normalized();
    }

    private void SetBoneCollisionShapesDisabled(bool disabled) => 
        _boneCollisionShapes.ForEach(cs => cs.Disabled = disabled);

    public void StartRagdoll()
    {
        SetBoneCollisionShapesDisabled(false);
        CollisionShape3D.Disabled = true;
        PhysicalBoneSimulator3D.Active = true;
        PhysicalBoneSimulator3D.PhysicalBonesStartSimulation();
        if (affectedBone is not null)
        {
            affectedBone.LinearVelocity = dirLastDamage * 20f;
        }
    }
}
