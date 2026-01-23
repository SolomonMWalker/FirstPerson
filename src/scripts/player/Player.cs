using System;
using FirstPerson.CustomTypes;
using FirstPerson.Helpers;
using FirstPerson.Scenes.Player.PlayerState;
using Godot;

namespace FirstPerson.Scenes.Player;

public partial class Player : HittableCharacterBody3D
{
    [Export] public CameraController CameraController { get; set; }
    [Export] public CameraEffects CameraEffects { get; set; }
    [Export] public PlayerStateMachine PlayerStateMachine { get; set; }
    [Export] public ClamberController ClamberController { get; set; }
    [Export] public StepHandlerComponent StepHandlerComponent { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public CollisionShape3D StandingCollisionShape { get; set; }
    [Export] public CollisionShape3D CrouchingCollisionShape { get; set; }
    [Export] public Node3D BottomOfPlayer { get; set; }
    [Export] public float CameraSensitivity { get; set; } = 0.01f;
    [Export] public float Speed { get; set; } = 8;
    [Export] public float JumpVelocity { get; set; } = 5f;
    [Export] public float FallVelocityThreshold { get; set; } = -7.0f;
    [Export] public int HealthDamage { get; set; } = 10;
    [Export] public int StaggerDamage { get; set; } = 10;
    [Export] public int ShootRaycastLength { get; set; } = 50;
    [Export] public int InteractRaycastLength { get; set; } = 50;
    [Export] public float InteractRaycastWaitInSec { get; set; } = 0.2f;
    [Export] public float DefaultCollisionShapePositionY { get; set; }
    [Export] public float AccelerationFactor { get; set; } = 0.9f;
    [Export] public float CrouchAnimationInSeconds { get; set; } = 0.25f;
    [Export] public float CrouchMovementMult { get; set; } = 0.6f;
    [Export] public float DefaultFov { get; set; }
    [Export] public float SprintFovMult { get; set; } = 1.05f;
    [Export] public float SprintTransitionAnimationInSeconds { get; set; } = 0.15f;
    [Export] public float SprintMovementMult { get; set; } = 1.5f;
    [Export] public float CoyoteTimeInSec { get; set; } = 0.15f;
    [Export] public float ClamberVelocity { get; set; } = 10f;
    
    public Vector2 InputDirections = Vector2.Zero;
    public float DefaultMovementMult { get; private set; } = 1f;
    public float CurrentMovementMult { get; set; }
    public bool InAir { get; set; }
    public bool Clambering { get; set; }
    public float CurrentFallVelocity { get; set; }
    public Vector3 PreviousFrameVelocity { get; set; }
    
    private float Gravity { get; } = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    private float ClamberXzDistanceSquared { get; set; }
    private Vector3 ClamberDestination { get; set; }
    private Vector2 ClamberDestinationXz { get; set; }
    private Vector3 ClamberStartPoint { get; set; }
    private Vector2 ClamberStartPointXz { get; set; }
    private Vector2 ClamberXzDirection { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("Fire"))
        {
            HandleFire();
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
            Clamber();
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

    public void Jump()
    {
        Velocity = Velocity with { Y = JumpVelocity };
    }

    private void Clamber()
    {
        if (BottomOfPlayer.GlobalPosition.Y < ClamberDestination.Y + ClamberController.ClamberMargin)
        { //move up to clamber Y
            Velocity = Vector3.Up * ClamberVelocity;
            MoveAndSlide();
            return;
        }
        if (ClamberXzDistanceSquared > ClamberStartPointXz.DistanceSquaredTo(new Vector2(GlobalPosition.X, GlobalPosition.Z)))
        { //move forward to clamber Z
            Velocity = new Vector3(ClamberXzDirection.X, 0, ClamberXzDirection.Y) * ClamberVelocity;
            MoveAndSlide();
            return;
        }
        Clambering = false;
    }

    public bool TryHandleClamber()
    {
        var clamberCheck = ClamberController.AttemptClamber();
        if (!clamberCheck.success) return false;
        ClamberDestination = clamberCheck.result.GlobalPositionToClamberTo ?? Vector3.Zero;
        ClamberDestinationXz = new Vector2(ClamberDestination.X, ClamberDestination.Z);
        ClamberStartPoint = GlobalPosition;
        ClamberStartPointXz = new Vector2(GlobalPosition.X, GlobalPosition.Z);
        ClamberXzDirection = ClamberStartPointXz
            .DirectionTo(new Vector2(ClamberDestination.X, ClamberDestination.Z));
        ClamberXzDistanceSquared = ClamberStartPointXz.DistanceSquaredTo(ClamberDestinationXz);
        return true;
    }

    private void HandleFire()
    {
        if (AnimationPlayer.IsPlaying() && AnimationPlayer.CurrentAnimation == "FireGun") return;
        AnimationPlayer.Play("FireGun");
        var collided = CameraController.GetWhatShootRaycastIsHitting();
        if (collided is null) return;
        var hitParams = new HitParameters(HealthDamage, StaggerDamage);
        switch (collided)
        {
            case Hitbox hitbox:
                hitbox.Hit(hitParams);
                break;
            case Weakspot weakspot:
                weakspot.Hit(hitParams);
                break;
        }
    }

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