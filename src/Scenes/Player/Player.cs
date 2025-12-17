using System;
using Godot;

namespace FirstPerson;

public partial class Player : HittableCharacterBody3D
{
    [Export] public float CameraSensitivity { get; set; } = 0.01f;
    [Export] public float Speed { get; set; } = 8;
    [Export] public float JumpVelocity { get; set; } = 5f;
    [Export] public int HealthDamage { get; set; } = 10;
    [Export] public int StaggerDamage { get; set; } = 10;
    [Export] public int ShootRaycastLength { get; set; } = 50;
    [Export] public int InteractRaycastLength { get; set; } = 50;
    [Export] public float InteractRaycastWaitInSec { get; set; } = 0.2f;
    [Export] public float DefaultCollisionShapePositionY { get; set; }
    [Export] public float CrouchCameraHeightMult { get; set; } = 0.4f;
    [Export] public float CrouchCollisionShapeHeightMult { get; set; } = 0.5f;
    [Export] public float CrouchAnimationInSeconds { get; set; } = 0.25f;
    [Export] public float CrouchMovementMult { get; set; } = 0.6f;
    [Export] public float DefaultFov { get; set; }
    [Export] public float SprintFovMult { get; set; } = 1.05f;
    [Export] public float SprintTransitionAnimationInSeconds { get; set; } = 0.15f;
    [Export] public float SprintMovementMult { get; set; } = 1.5f;
    [Export] public float CoyoteTimeInSec { get; set; } = 0.15f;
    [Export] public float ClamberVelocity { get; set; } = 10f;
    
    private float Gravity { get; } = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    
    private float ClamberXZDistanceSquared { get; set; }
    private Vector3 ClamberDestination { get; set; }
    private Vector2 ClamberDestinationXZ { get; set; }
    private Vector3 ClamberStartPoint { get; set; }
    private Vector2 ClamberStartPointXZ { get; set; }
    private Vector2 ClamberXZDirection { get; set; }

    private double TimeSinceLastInteractCheck { get; set; }
    private double TimeInCoyoteTime { get; set; }
    private bool CanJump { get; set; }
    private bool FireCameraRaycast { get; set; }
    private bool RotateCamera { get; set; }
    private Vector2 RelativeMousePosition { get; set; }
    private float DefaultCameraHeight { get; set; }
    private float DefaultColliderShapeHeight { get; set; }
    
    private Camera3D Camera { get; set; }
    private Node3D Hand { get; set; }
    private RayCast3D ShootRaycast { get; set; }
    private RayCast3D InteractRaycast { get; set; }
    private CollisionShape3D CollisionShape3d { get; set; }
    private BoxShape3D CollisionBoxShape { get; set; }
    private AnimationPlayer AnimationPlayer { get; set; }
    private ClamberController ClamberController { get; set; }
    private Tween EnterCrouchTween { get; set; }
    private Tween ExitCrouchTween { get; set; }

    private PlayerMovementState CurrentMovementState { get; set; } = PlayerMovementState.Default;
    private PlayerActionState CurrentPlayerActionState { get; set; } = PlayerActionState.OnFloor;

    
    /*
     * Need to create headbob animations
     * different ones for each movement state
     * when the player leaves each state, need to blend animation to new state
     * Could make a number correspond to where you are in the animation to help transition to new state
     * 1 - start left from middle, 2 - going left, 3 - left maximum, start right
     * 4 - going right from right, 5 - in middle going right, 6 - going right
     * 7 - right maximum, start left, 8 - going left, 1 - start left from middle
     * might still have to use animation tree
     * https://docs.godotengine.org/en/stable/tutorials/animation/animation_tree.html#
     *
     * In trenchbroom, create a map, then put skip texture on all faces except
     * faces used in navigation
     * layer maps together and use the navigation mesh to make the nav map
     *
     * look into how crouching affects clamber raycast layout
     * probably need to use scale as oppposed to something else
     */

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        CollisionShape3d = GetNode<CollisionShape3D>("CollisionShape3D");
        ClamberController = GetNode<ClamberController>("CollisionShape3D/ClamberController");
        Camera = GetNode<Camera3D>("Camera3D");
        ShootRaycast = Camera.GetNode<RayCast3D>("ShootRayCast");
        ShootRaycast.AddException(this);
        ShootRaycast.SetTargetPosition(Vector3.Forward * ShootRaycastLength);
        InteractRaycast = Camera.GetNode<RayCast3D>("InteractRayCast");
        InteractRaycast.AddException(this);
        InteractRaycast.SetTargetPosition(Vector3.Forward * InteractRaycastLength);
        Hand = Camera.GetNode<Node3D>("Hand");
        DefaultCameraHeight = Camera.Position.Y;
        CollisionBoxShape = (BoxShape3D)CollisionShape3d.Shape;
        DefaultColliderShapeHeight = CollisionBoxShape.Size.Y;
        DefaultCollisionShapePositionY = CollisionShape3d.Position.Y;
        DefaultFov = Camera.Fov;
    }
    
    public float GetBottomOfCharacter() => GlobalPosition.Y - CollisionBoxShape.Size.Y / 2;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("Fire"))
        {
            AnimationPlayer.Play("FireGun");
            FireCameraRaycast = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleInteractCheck(delta);
        HandleFire();
        if (CurrentPlayerActionState == PlayerActionState.Clambering)
        {
            Clamber();
            return;
        }
        if (CurrentPlayerActionState == PlayerActionState.CoyoteTime)
        {
            TimeInCoyoteTime += delta;
            if (TimeInCoyoteTime > CoyoteTimeInSec)
            {
                CanJump = false;
                CurrentPlayerActionState = PlayerActionState.InAir;
            }
        }
        HandleCrouch();
        HandleSprint();
        var movementInput = GetXZDirectionalMovement();
        var tempVelocity = Vector3.Zero;
        if (IsOnFloor())
        {
            if (!CanJump) CanJump = true;
            if (CurrentPlayerActionState == PlayerActionState.InAir)
            {
                CurrentPlayerActionState = PlayerActionState.OnFloor;
            }
            if (Input.IsActionJustPressed("Jump") && CanJump)
            {
                tempVelocity.Y = JumpVelocity;
                CanJump = false;
            }
        }
        else
        {
            if (CanJump && CurrentPlayerActionState != PlayerActionState.CoyoteTime)
            {
                CurrentPlayerActionState = PlayerActionState.CoyoteTime;
                TimeInCoyoteTime = 0;
            }
            else if(CurrentPlayerActionState != PlayerActionState.CoyoteTime &&
                    CurrentPlayerActionState != PlayerActionState.InAir)
            {
                CurrentPlayerActionState = PlayerActionState.InAir;
            }
            tempVelocity.Y = (float) (Velocity.Y - Gravity * delta);
            if(Input.IsActionPressed("Jump")) //Clamber
            {
                if (TryHandleClamber()) return;
                if (Input.IsActionJustPressed("Jump") && CurrentPlayerActionState is PlayerActionState.CoyoteTime)
                {
                    tempVelocity.Y = JumpVelocity;
                    CanJump = false;
                    CurrentPlayerActionState = PlayerActionState.InAir;
                }
            }
        }
        var movementMult = CurrentMovementState switch
        {
            PlayerMovementState.Crouching => CrouchMovementMult,
            PlayerMovementState.Sprinting => SprintMovementMult,
            _ => 1f
        };
        //awesome reference https://git.colormatic.org/ColormaticStudios/quality-godot-first-person/src/branch/main/addons/fpc/character.gd
        var directionV2 = movementInput.Rotated(-Camera.Rotation.Y);
        tempVelocity.X = directionV2.X * Speed * movementMult;
        tempVelocity.Z = directionV2.Y * Speed * movementMult;
        Velocity = tempVelocity;
        MoveAndSlide();

        if (RotateCamera)
        {
            RotateCamera = false;
            LookAtMouse();
        }
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            RelativeMousePosition = mouseMotionEvent.Relative;
            RotateCamera = true;
        }
    }

    public void LookAtMouse()
    {
        var lookDir = RelativeMousePosition;
        var rotationY = Camera.Rotation.Y - lookDir.X * CameraSensitivity;
        var rotationX = Math.Clamp(Camera.Rotation.X - lookDir.Y * CameraSensitivity, 
            Mathf.DegToRad(-90), Mathf.DegToRad(90));
        Camera.SetRotation(new Vector3(rotationX, rotationY, 0));
        CollisionShape3d.SetRotation(new Vector3(0, Camera.Rotation.Y, 0));
    }
    
    public void Clamber()
    {
        if (GetBottomOfCharacter() < ClamberDestination.Y + ClamberController.ClamberMargin)
        { //move up to clamber Y
            Velocity = Vector3.Up * ClamberVelocity;
            MoveAndSlide();
            return;
        }
        if (ClamberXZDistanceSquared > ClamberStartPointXZ.DistanceSquaredTo(new Vector2(GlobalPosition.X, GlobalPosition.Z)))
        { //move forward to clamber Z
            Velocity = new Vector3(ClamberXZDirection.X, 0, ClamberXZDirection.Y) * ClamberVelocity;
            MoveAndSlide();
            return;
        }
        ApplyFloorSnap(); //when done, switch movement type to onfloor
        CurrentPlayerActionState = PlayerActionState.OnFloor;
    }

    public Vector2 GetXZDirectionalMovement()
    {
        var movementInput = Vector2.Zero;
        if (Input.IsActionPressed("MoveForward")) //forward
        {
            movementInput += Vector2.Up;
        }
        if (Input.IsActionPressed("MoveBackward")) //backward
        {
            movementInput += Vector2.Down;
        }
        if (Input.IsActionPressed("MoveRight")) //right
        {
            movementInput += Vector2.Right;
        }
        if (Input.IsActionPressed("MoveLeft")) //left
        {
            movementInput += Vector2.Left;
        }

        return movementInput;
    }

    public void HandleCrouch()
    {
        if (!Input.IsActionJustPressed("Crouch") || !IsOnFloor()) return;
        if (CurrentMovementState == PlayerMovementState.Crouching)
        {
            PlayExitCrouchAnim();
            CurrentMovementState = PlayerMovementState.Default;
        }
        else
        {
            PlayEnterCrouchAnim();
            CurrentMovementState = PlayerMovementState.Crouching;
        }
    }

    public void HandleSprint()
    {
        if (Input.IsActionJustPressed("Sprint") && IsOnFloor())
        {
            if (CurrentMovementState == PlayerMovementState.Sprinting)
            {
                PlayExitSprintAnim();
                CurrentMovementState = PlayerMovementState.Default;
            }
            else
            {
                PlayEnterSprintAnim();
                CurrentMovementState = PlayerMovementState.Sprinting;
            }
        }
        else if (CurrentMovementState == PlayerMovementState.Sprinting && Velocity == Vector3.Zero)
        {
            PlayExitSprintAnim();
            CurrentMovementState = PlayerMovementState.Default;
        }
    }

    public bool TryHandleClamber()
    {
        var clamberCheck = ClamberController.AttemptClamber();
        if (!clamberCheck.success) return false;
        CurrentPlayerActionState = PlayerActionState.Clambering;
        ClamberDestination = clamberCheck.result.globalPositionToClamberTo ?? Vector3.Zero;
        ClamberDestinationXZ = new Vector2(ClamberDestination.X, ClamberDestination.Z);
        ClamberStartPoint = GlobalPosition;
        ClamberStartPointXZ = new Vector2(GlobalPosition.X, GlobalPosition.Z);
        ClamberXZDirection = ClamberStartPointXZ
            .DirectionTo(new Vector2(ClamberDestination.X, ClamberDestination.Z));
        ClamberXZDistanceSquared = ClamberStartPointXZ.DistanceSquaredTo(ClamberDestinationXZ);
        return true;
    }

    public void HandleFire()
    {
        if (!FireCameraRaycast) return;
        FireCameraRaycast = false;
        if (!ShootRaycast.IsColliding()) return;
        var collided = ShootRaycast.GetCollider();
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

    public void HandleInteractCheck(double delta)
    {
        if (TimeSinceLastInteractCheck < InteractRaycastWaitInSec)
        {
            TimeSinceLastInteractCheck += delta;
            return;
        }
        TimeSinceLastInteractCheck = 0;
        
        if (!InteractRaycast.IsColliding()) return;
        //if interactable is on screen, turn on interact prompt
    }

    public void PlayEnterCrouchAnim()
    {
        EnterCrouchTween = GetTree().CreateTween();
        EnterCrouchTween.TweenProperty(CollisionBoxShape, "size:y",
            DefaultColliderShapeHeight * CrouchCollisionShapeHeightMult, CrouchAnimationInSeconds);
        EnterCrouchTween.TweenProperty(CollisionShape3d, "position:y",
            DefaultCollisionShapePositionY * CrouchCollisionShapeHeightMult, CrouchAnimationInSeconds);
    }

    public void PlayExitCrouchAnim()
    {
        ExitCrouchTween = GetTree().CreateTween();
        ExitCrouchTween.TweenProperty(CollisionBoxShape, "size:y",
            DefaultColliderShapeHeight, CrouchAnimationInSeconds);
        ExitCrouchTween.TweenProperty(CollisionShape3d, "position:y",
            DefaultCollisionShapePositionY, CrouchAnimationInSeconds);
    }

    public void PlayEnterSprintAnim()
    {
        var tween = CreateTween();
        tween.TweenProperty(Camera, "fov", DefaultFov * SprintFovMult, SprintTransitionAnimationInSeconds);
    }

    public void PlayExitSprintAnim()
    {
        var tween = CreateTween();
        tween.TweenProperty(Camera, "fov", DefaultFov, SprintTransitionAnimationInSeconds);
    }

    public override void Hit(HitParameters hitParameters)
    {
        base.Hit(hitParameters);
        AnimationPlayer.Play("Shot");   
    }
}