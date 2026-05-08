using Godot;
using System;
using System.Collections.Generic;
using FirstPerson.agents.enemies.testDog;

public partial class Dog : CombatAgent
{
    [ExportCategory("Dog Settings")]
    [Export] public float AttackRate { get; set; } = 2.0f;

    [Export] public float LeapAttackSpeed { get; set; } = 25;
    [Export] public float LeapAttackJumpForce { get; set; } = 15;

    [ExportCategory("Components")]
    [Export] public LeapAttackAiComponent LeapAttackAiComponent { get; set; }
    [Export] public AgentZigzagComponent AgentZigzagComponent { get; set; }
    [Export] public AgentFollowComponent AgentFollowComponent { get; set; }
    [Export] public AgentStopComponent AgentStopComponent { get; set; }
    
    [ExportCategory("References")]
    [Export] public CustomAnimationTree CustomAnimationTree { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public ShapeCast3D ShouldCloseAttackShapeCast { get; set; }
    [Export] public ShapeCast3D ShouldLeapAttackShapeCast { get; set; }
    [Export] public ShapeCast3D CloseAttackRangeShapeCast { get; set; }
    [Export] public FuzzyStartTimer AttackRateTimer { get; set; }

    [ExportCategory("Paths")]
    [Export] public StringName CombatStateSwitchStateMachinePath { get; set; }
    [Export] public StringName InCombatStateMachinePath { get; set; }
    [Export] public StringName NotInCombatStateMachinePath { get; set; }

    public AnimationNodeStateMachinePlayback combatStateSwitchStateMachine,
        inCombatStateMachine,
        notInCombatStateMachine;

    public bool nextAttackIsLeap = false;
    public bool meleeAttacking, leapAttacking, leapAttackInProgress, leapAttackDone;
    public bool Attacking { get => _attacking; }
    public bool RotationEnabled { get; set; }
    public float leapAttackOffset;
    private bool _attacking;
    private List<GodotObject> meleeAttackObjectsHit = [];
    public override bool CanRotate() => base.CanRotate() && RotationEnabled;
    public void DisableRotation() => RotationEnabled = false;
    public void EnableRotation() => RotationEnabled = true;
    public void StartAttacking() => _attacking = true;

    public void StopAttacking()
    {
        _attacking = false;
        CloseAttackRangeShapeCast.Enabled = false;
    }

    public void EnableMeleeHitbox() => CloseAttackRangeShapeCast.Enabled = true;
    public void DisableMeleeHitbox() => CloseAttackRangeShapeCast.Enabled = false;
    public void EnableLeapAttackTrigger() => ShouldLeapAttackShapeCast.Enabled = true;
    public void DisableLeapAttackTrigger() => ShouldLeapAttackShapeCast.Enabled = false;
    public void EnableMeleeAttackTrigger() => ShouldCloseAttackShapeCast.Enabled = true;
    public void DisableMeleeAttackTrigger() => ShouldCloseAttackShapeCast.Enabled = false;
    
    public override void SetLastDamageDirection(Vector3 sourceGlobalPosition, Vector3 collisionGlobalPoint)
    {
        base.SetLastDamageDirection(sourceGlobalPosition, collisionGlobalPoint);
        CustomAnimationTree.TrySetParam("blend_position", dirLastDamageXz);
        CustomAnimationTree.TrySetParam("request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
    
    public override void _Ready()
    {
        base._Ready();
        CustomAnimationTree.Active = true;
        combatStateSwitchStateMachine = (AnimationNodeStateMachinePlayback)
            CustomAnimationTree.Get(CombatStateSwitchStateMachinePath);
        inCombatStateMachine = (AnimationNodeStateMachinePlayback)
            CustomAnimationTree.Get(InCombatStateMachinePath);
        notInCombatStateMachine = (AnimationNodeStateMachinePlayback)
            CustomAnimationTree.Get(NotInCombatStateMachinePath);
        defaultCombatAi = AgentZigzagComponent;
        defaultNoncombatAi = AgentStopComponent;
        CurrentNavComponent = AgentStopComponent;

        AttackRateTimer.SetStartTime(AttackRate);
        AttackRateTimer.FuzzyStart();
        AttackRateTimer.Timeout += () =>
        {
            if (nextAttackIsLeap)
            {
                ShouldLeapAttackShapeCast.Enabled = true;
            }
            else
            {
                ShouldCloseAttackShapeCast.Enabled = true;
            }
        };

        ShouldCloseAttackShapeCast.Enabled = true;
        DebugMenu.Register("DogState", () => EnemyStateMachine.GetStateMachineString());
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (nextAttackIsLeap)
        {
            HandleLeapAttackStart();
        }
        else
        {
            HandleCloseMeleeAttackStart();
        }
        HandleMeleeAttackCollision();
    }

    public override void StartRagdoll()
    {
        CustomAnimationTree.Active = false;
        base.StartRagdoll();
        AnimationPlayer.Stop();
        AnimationPlayer.QueueFree();
    }

    public void HandleCloseMeleeAttackStart()
    {
        if (!_attacking && !meleeAttacking && ShouldCloseAttackShapeCast.Enabled 
            && ShouldCloseAttackShapeCast.IsColliding())
        {
            meleeAttackObjectsHit = [];
            meleeAttacking = true;
            Velocity = Vector3.Zero;
            NavigationAgent3D.Velocity = Vector3.Zero;
            CurrentNavComponent = AgentStopComponent;
        }
    }

    public void HandleLeapAttackStart()
    {
        if (!_attacking && !leapAttacking && ShouldLeapAttackShapeCast.Enabled
            && ShouldLeapAttackShapeCast.IsColliding())
        {
            meleeAttackObjectsHit = [];
            leapAttacking = true;
            Velocity = Vector3.Zero;
            NavigationAgent3D.Velocity = Vector3.Zero;
            LeapAttackAiComponent.StartLeapAttack();
            SetCurrentNavComponent(LeapAttackAiComponent);
        }
    }

    public void StartLeapAttackMovement()
    {
        leapAttackInProgress = true;
        LeapAttackAiComponent.CheckGroundedTimer.Start();
    }

    public void HandleMeleeAttackCollision()
    {
        if (!CloseAttackRangeShapeCast.IsColliding()) return;
        for (int i = 0; i < CloseAttackRangeShapeCast.GetCollisionCount(); i++)
        {
            var collided = CloseAttackRangeShapeCast.GetCollider(i);
            if (collided is Hitbox hitbox && !meleeAttackObjectsHit.Contains(hitbox.Parent))
            {
                hitbox.Hit(BuildHitInformation(CloseAttackRangeShapeCast.GetCollisionPoint(i)));
                meleeAttackObjectsHit.Add(hitbox.Parent);
            }
        }
    }
}
