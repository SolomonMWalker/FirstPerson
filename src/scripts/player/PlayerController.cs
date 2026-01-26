using System;
using FirstPerson.CustomTypes;
using FirstPerson.Helpers;
using FirstPerson.Scenes.Player.PlayerState;
using Godot;

namespace FirstPerson.Scenes.Player;

public partial class PlayerController : HittableCharacterBody3D
{
    [Export] public bool Debug { get; set; }
    
    [ExportCategory("References")]
    [Export] public CameraController CameraController { get; set; }
    [Export] public CameraEffects CameraEffects { get; set; }
    [Export] public PlayerStateMachine PlayerStateMachine { get; set; }
    [Export] public ClamberController ClamberController { get; set; }
    [Export] public StepHandlerComponent StepHandlerComponent { get; set; }
    [Export] public WeaponController WeaponController { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public CollisionShape3D StandingCollisionShape { get; set; }
    [Export] public CollisionShape3D CrouchingCollisionShape { get; set; }
    [Export] public Node3D BottomOfPlayer { get; set; }
    
    [ExportCategory("Player Settings")]
    [Export] public float Speed { get; set; } = 8;
    [Export] public float JumpVelocity { get; set; } = 5f;
    [Export] public float FallVelocityThreshold { get; set; } = -7.0f;
    [Export] public float AccelerationFactor { get; set; } = 0.9f;
    [Export] public float CrouchMovementMult { get; set; } = 0.6f;
    [Export] public float SprintMovementMult { get; set; } = 1.5f;
    [Export] public float ClamberVelocity { get; set; } = 10f;
    
    public Vector2 InputDirections = Vector2.Zero;
    public float DefaultMovementMult { get; private set; } = 1f;
    public float CurrentMovementMult { get; set; } = 1f;
    public bool InAir { get; set; }
    public bool Clambering { get; set; }
    public float CurrentFallVelocity { get; set; }
    public Vector3 PreviousFrameVelocity { get; set; }
    
    private float Gravity { get; } = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("Fire"))
        {
            //HandleFire();
        }
        HandleInteractCheck(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        PreviousFrameVelocity = Velocity;
        HandleMovement(delta);
    }

    public override void Hit(HitParameters hitParameters)
    {
        base.Hit(hitParameters);
        AnimationPlayer.Play("Shot");
    }

    private void HandleMovement(double delta)
    {
        if (Clambering)
        {
            ClamberController.Clamber();
            return;
        }

        var movementInput = Input.GetVector(
            "MoveLeft", "MoveRight", "MoveForward", "MoveBackward");
        InputDirections = movementInput;
        
        var yVelocity = Velocity.Y;
        if (InAir)
        {
            yVelocity -= Gravity * (float) delta;
        }

        var direction = (Transform.Basis * new Vector3(movementInput.X, 0, movementInput.Y)).Normalized();
        if (direction.IsZeroApprox())
        {
            Velocity = new Vector3(0f, yVelocity, 0f);
        }
        else
        {
            var xzVelocity = new Vector2(direction.X, direction.Z) * Speed * CurrentMovementMult;
            xzVelocity = new Vector2(Velocity.X, Velocity.Z).Lerp(xzVelocity, AccelerationFactor);
            Velocity = new Vector3(xzVelocity.X, yVelocity, xzVelocity.Y);
        }
        MoveAndSlide();
        
        if (IsOnFloor())
        {
            StepHandlerComponent.HandleStepClimbing();
        }
    }

    public void UpdateRotation(Vector3 newRotation)
    {
        GlobalTransform = GlobalTransform with { Basis = Basis.FromEuler(newRotation) };
    }

    public void Jump() => Velocity = Velocity with { Y = JumpVelocity };

    // private void HandleFire()
    // {
    //     if (AnimationPlayer.IsPlaying() && AnimationPlayer.CurrentAnimation == "FireGun") return;
    //     AnimationPlayer.Play("FireGun");
    //     var collided = CameraController.GetWhatShootRaycastIsHitting();
    //     if (collided is null) return;
    //     var hitParams = new HitParameters(HealthDamage, StaggerDamage);
    //     switch (collided)
    //     {
    //         case Hitbox hitbox:
    //             hitbox.Hit(hitParams);
    //             break;
    //         case Weakspot weakspot:
    //             weakspot.Hit(hitParams);
    //             break;
    //     }
    // }

    private void HandleInteractCheck(double delta)
    {
        // if (InteractCheckPoll.IsPollPinged(delta))
        // {
        //     if (!InteractRaycast.IsColliding()) return;
        //     if interactable is on screen, turn on interact prompt
        // }        
    }

    public bool CheckFallSpeed()
    {
        if (CurrentFallVelocity < FallVelocityThreshold)
        {
            CurrentFallVelocity = 0;
            return true;
        }

        CurrentFallVelocity = 0;
        return false;
    }
}