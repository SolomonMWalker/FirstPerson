using System;
using Godot;

namespace FirstPerson;

public partial class Player : CharacterBody3D
{
    public Camera3D Camera;
    public RayCast3D SightRaycast;
    public CollisionShape3D CollisionShape3d;
    public BoxShape3D CollisionBoxShape;
    public AnimationPlayer AnimationPlayer;
    public float cameraSensitivity = 0.01f;
    public float speed = 10;
    public float jumpVelocity = 6.5f;
    public float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    public float defaultCollisionShapePositionY;
    public float crouchCameraHeightMult = 0.4f;
    public float crouchCollisionShapeHeightMult = 0.5f;
    public float crouchAnimationInSeconds = 0.25f;
    public Tween enterCrouchTween;
    public Tween exitCrouchTween;

    private float defaultCameraHeight;
    private float defaultColliderShapeHeight;

    private MovementState currentMovementState = MovementState.Walking;

    public enum MovementState
    {
        Walking,
        Crouching,
        Sprinting,
        InAir,
        CoyoteTime //InAir, but can still jump
    }

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        SightRaycast = GetNode<RayCast3D>("Camera3D/RayCast3D");
        CollisionShape3d = GetNode<CollisionShape3D>("CollisionShape3D");
        Camera = GetNode<Camera3D>("Camera3D");
        defaultCameraHeight = Camera.Position.Y;
        CollisionBoxShape = (BoxShape3D)CollisionShape3d.Shape;
        defaultColliderShapeHeight = CollisionBoxShape.Size.Y;
        defaultCollisionShapePositionY = CollisionShape3d.Position.Y;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleCrouch();
        var movementInput = Vector2.Zero;
        if (Input.IsKeyPressed(Key.W)) //forward
        {
            movementInput += Vector2.Up;
        }
        if (Input.IsKeyPressed(Key.S)) //backward
        {
            movementInput += Vector2.Down;
        }
        if (Input.IsKeyPressed(Key.D)) //right
        {
            movementInput += Vector2.Right;
        }
        if (Input.IsKeyPressed(Key.A)) //left
        {
            movementInput += Vector2.Left;
        }
        
        var tempVelocity = Vector3.Zero;

        if (IsOnFloor())
        {
            if (Input.IsActionJustPressed("jump"))
            {
                tempVelocity.Y = jumpVelocity;
            }
        }
        else
        {
            tempVelocity.Y = (float) (Velocity.Y - gravity * delta);
        }
        
        //awesome reference https://git.colormatic.org/ColormaticStudios/quality-godot-first-person/src/branch/main/addons/fpc/character.gd

        var directionV2 = movementInput.Rotated(-Camera.Rotation.Y);
        tempVelocity.X = directionV2.X * speed;
        tempVelocity.Z = directionV2.Y * speed;
        Velocity = tempVelocity;
        MoveAndSlide();
    }

    public void HandleCrouch()
    {
        //this needs to be "action just pressed"
        if (Input.IsActionJustPressed("crouch") && IsOnFloor())
        {
            if (currentMovementState == MovementState.Crouching)
            {
                PlayExitCrouchAnim();
                currentMovementState = MovementState.Walking;
            }
            else
            {
                PlayEnterCrouchAnim();
                currentMovementState = MovementState.Crouching;
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            var lookDir = mouseMotionEvent.Relative;
            var rotationY = Camera.Rotation.Y - lookDir.X * cameraSensitivity;
            var rotationX = Math.Clamp(Camera.Rotation.X - lookDir.Y * cameraSensitivity, 
                Mathf.DegToRad(-90), Mathf.DegToRad(90));
            Camera.SetRotation(new Vector3(rotationX, rotationY, 0));
        }
        else if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                AnimationPlayer.Play("FireGun");
                if (SightRaycast.CollideWithBodies)
                {
                    var body = SightRaycast.GetCollider();
                    //if body is something that reacts to being shot
                    //shoot it
                }
            }
        }
    }

    public void PlayEnterCrouchAnim()
    {
        enterCrouchTween = GetTree().CreateTween();
        enterCrouchTween.TweenProperty(CollisionBoxShape, "size:y",
            defaultColliderShapeHeight * crouchCollisionShapeHeightMult, crouchAnimationInSeconds);
        enterCrouchTween.TweenProperty(CollisionShape3d, "position:y",
            defaultCollisionShapePositionY * crouchCollisionShapeHeightMult, crouchAnimationInSeconds);
    }

    public void PlayExitCrouchAnim()
    {
        exitCrouchTween = GetTree().CreateTween();
        exitCrouchTween.TweenProperty(CollisionBoxShape, "size:y",
            defaultColliderShapeHeight, crouchAnimationInSeconds);
        exitCrouchTween.TweenProperty(CollisionShape3d, "position:y",
            defaultCollisionShapePositionY, crouchAnimationInSeconds);
    }
}