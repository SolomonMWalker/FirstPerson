using System;
using FirstPerson.CustomTypes;
using FirstPerson.Helpers;
using Godot;

namespace FirstPerson.Scenes.Player;

public partial class Player : HittableCharacterBody3D
{
    [Export] public CameraController CameraController { get; set; }
    [Export] public CameraEffects CameraEffects { get; set; }
    [Export] public PlayerStateMachine PlayerStateMachine { get; set; }
    [Export] public ClamberController ClamberController { get; set; }
    [Export] public StepHandlerComponent StepHandlerComponent { get; set; }
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
    //[Export] public float CrouchCameraHeightMult { get; set; } = 0.4f;
    //[Export] public float CrouchCollisionShapeHeightMult { get; set; } = 0.5f;
    [Export] public float CrouchAnimationInSeconds { get; set; } = 0.25f;
    [Export] public float CrouchMovementMult { get; set; } = 0.6f;
    [Export] public float DefaultFov { get; set; }
    [Export] public float SprintFovMult { get; set; } = 1.05f;
    [Export] public float SprintTransitionAnimationInSeconds { get; set; } = 0.15f;
    [Export] public float SprintMovementMult { get; set; } = 1.5f;
    [Export] public float CoyoteTimeInSec { get; set; } = 0.15f;
    [Export] public float ClamberVelocity { get; set; } = 10f;
    public Vector2 InputDirections = Vector2.Zero;
    
    private float Gravity { get; } = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    
    private float ClamberXzDistanceSquared { get; set; }
    private Vector3 ClamberDestination { get; set; }
    private Vector2 ClamberDestinationXz { get; set; }
    private Vector3 ClamberStartPoint { get; set; }
    private Vector2 ClamberStartPointXz { get; set; }
    private Vector2 ClamberXzDirection { get; set; }

    private Poll InteractCheckPoll { get; set; }
    private Poll CoyoteTimePoll { get; set; }
    private bool CanJump { get; set; }
    private bool FireCameraRaycast { get; set; }
    private bool RotateCamera { get; set; }
    private Vector2 RelativeMousePosition { get; set; }
    //private float DefaultCameraHeight { get; set; }
    //private float DefaultColliderShapeHeight { get; set; }
    
    private Camera3D Camera { get; set; }
    //private Node3D Hand { get; set; }
    private RayCast3D ShootRaycast { get; set; }
    private RayCast3D InteractRaycast { get; set; }
    private CollisionShape3D CollisionShape3d { get; set; }
    private BoxShape3D CollisionBoxShape { get; set; }
    private AnimationPlayer AnimationPlayer { get; set; }
    private Tween EnterCrouchTween { get; set; }
    private Tween ExitCrouchTween { get; set; }
    
    public bool Jumped { get; set; }
    public bool Clambering { get; set; }
    public bool Sprinting { get; set; }
    public bool Crouching { get; set; }
    public float CurrentFallVelocity { get; set; }
    public Vector3 PreviousFrameVelocity { get; set; }

    
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
        CoyoteTimePoll = new Poll(CoyoteTimeInSec);
        InteractCheckPoll = new Poll(InteractRaycastWaitInSec);
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

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
        var movementState = PlayerStateMachine.GetMovementState();
        var airborneState = PlayerStateMachine.GetAirborneState();
        if (Clambering)
        {
            Clamber();
            return;
        }

        var movementInput = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");
        InputDirections = movementInput;
        HandleCrouch(movementState, airborneState);
        HandleSprint(movementState, airborneState);
        var tempVelocity = Vector3.Zero;
        bool jumpJustPressed = Input.IsActionJustPressed("Jump");
        bool jumpPressedAWhile = false;
        if (!jumpJustPressed)
        {
            jumpPressedAWhile = Input.IsActionPressed("Jump");
        }
        if (airborneState is "GroundedState")
        {
            if (jumpJustPressed)
            {
                tempVelocity.Y = JumpVelocity;
                Jumped = true;
            }
        }
        else if (airborneState is "CoyoteTimeState")
        {
            if (jumpJustPressed)
            {
                tempVelocity.Y = JumpVelocity;
                Jumped = true;
            }
            else
            {
                tempVelocity.Y = (float) (Velocity.Y - Gravity * delta);
            }
        }
        else
        {
            if(jumpJustPressed || jumpPressedAWhile)
            {
                if (TryHandleClamber())
                {
                    return;
                }
            }
            tempVelocity.Y = (float) (Velocity.Y - Gravity * delta);
        }
        var movementMult = movementState switch
        {
            "CrouchingState" => CrouchMovementMult,
            "SprintingState" => SprintMovementMult,
            _ => 1f
        };

        var direction = (Transform.Basis * new Vector3(movementInput.X, 0, movementInput.Y)).Normalized();
        if (direction.IsZeroApprox())
        {
            Velocity = new Vector3(0f, tempVelocity.Y, 0f);
        }
        else
        {
            //var currentXzVelocity = new Vector2(Velocity.X, Velocity.Z);
            var xzVelocity = new Vector2(direction.X, direction.Z) * Speed * movementMult;
            //xzVelocity = currentXzVelocity.Lerp(xzVelocity, 0.75f);
            Velocity = new Vector3(xzVelocity.X, tempVelocity.Y, xzVelocity.Y);
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
        ApplyFloorSnap(); //when done, switch movement type to onfloor
        Clambering = false;
    }

    private static Vector2 GetXzDirectionalMovement()
    {
        var movementInput = Vector2.Zero;
        if (Input.IsActionPressed("MoveForward")) //forward is negative z
        {
            movementInput += Vector2.Down;
        }
        if (Input.IsActionPressed("MoveBackward")) //backward is positive z
        {
            movementInput += Vector2.Up;
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

    private void HandleCrouch(string movementState, string airborneState)
    {
        if (!Input.IsActionJustPressed("Crouch") 
            || CameraController.IsEnterCrouchTweenRunning()
            || CameraController.IsExitCrouchTweenRunning()) return;
        if (!Crouching
            && movementState is not "CrouchingState" 
            && airborneState is "GroundedState")
        {
            Crouching = true;
        }
        else
        {
            Crouching = false;
        }
    }

    private void HandleSprint(string movementState, string airborneState)
    {
        if (!Sprinting
            && movementState is not "SprintingState" 
            && airborneState is "GroundedState"
            && InputDirections.LengthSquared() > 0
            && Input.IsActionPressed("Sprint"))
        {
            Sprinting = true;
        }
    }

    private bool TryHandleClamber()
    {
        var clamberCheck = ClamberController.AttemptClamber();
        if (!clamberCheck.success) return false;
        Clambering = true;
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

    public void PlayEnterSprintAnim()
    {
        //var tween = CreateTween();
        //tween.TweenProperty(Camera, "fov", DefaultFov * SprintFovMult, SprintTransitionAnimationInSeconds);
    }

    public void PlayExitSprintAnim()
    {
        //var tween = CreateTween();
        //tween.TweenProperty(Camera, "fov", DefaultFov, SprintTransitionAnimationInSeconds);
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