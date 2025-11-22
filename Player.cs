using System;
using FirstPerson.CustomTypes;
using Godot;

namespace FirstPerson;

public partial class Player : CharacterBody3D
{
    public Camera3D camera;
    public RayCast3D sightRaycast;
    public CollisionShape3D collisionShape3d;
    public BoxShape3D collisionBoxShape;
    public AnimationPlayer animationPlayer;
    public float cameraSensitivity = 0.01f;
    public float speed = 10;
    public float jumpVelocity = 6.5f;
    public float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    public float defaultCollisionShapePositionY;
    public float crouchCameraHeightMult = 0.4f;
    public float crouchCollisionShapeHeightMult = 0.5f;
    public float crouchAnimationInSeconds = 0.25f;
    public int shootRange = 50;
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
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        sightRaycast = GetNode<RayCast3D>("Camera3D/RayCast3D");
        collisionShape3d = GetNode<CollisionShape3D>("CollisionShape3D");
        camera = GetNode<Camera3D>("Camera3D");
        defaultCameraHeight = camera.Position.Y;
        collisionBoxShape = (BoxShape3D)collisionShape3d.Shape;
        defaultColliderShapeHeight = collisionBoxShape.Size.Y;
        defaultCollisionShapePositionY = collisionShape3d.Position.Y;
        sightRaycast.TargetPosition = new Vector3(0, 0, -shootRange);
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

        var directionV2 = movementInput.Rotated(-camera.Rotation.Y);
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
            var rotationY = camera.Rotation.Y - lookDir.X * cameraSensitivity;
            var rotationX = Math.Clamp(camera.Rotation.X - lookDir.Y * cameraSensitivity, 
                Mathf.DegToRad(-90), Mathf.DegToRad(90));
            camera.SetRotation(new Vector3(rotationX, rotationY, 0));
        }
        else if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                animationPlayer.Play("FireGun");
                if (sightRaycast.CollideWithBodies)
                {
                    var body = sightRaycast.GetCollider();
                    if (body is ShootableCharacterBody3D shootable)
                    {
                        shootable.Shot(new ShotParameters(1));
                    }
                }
            }
        }
    }

    public void PlayEnterCrouchAnim()
    {
        enterCrouchTween = GetTree().CreateTween();
        enterCrouchTween.TweenProperty(collisionBoxShape, "size:y",
            defaultColliderShapeHeight * crouchCollisionShapeHeightMult, crouchAnimationInSeconds);
        enterCrouchTween.TweenProperty(collisionShape3d, "position:y",
            defaultCollisionShapePositionY * crouchCollisionShapeHeightMult, crouchAnimationInSeconds);
    }

    public void PlayExitCrouchAnim()
    {
        exitCrouchTween = GetTree().CreateTween();
        exitCrouchTween.TweenProperty(collisionBoxShape, "size:y",
            defaultColliderShapeHeight, crouchAnimationInSeconds);
        exitCrouchTween.TweenProperty(collisionShape3d, "position:y",
            defaultCollisionShapePositionY, crouchAnimationInSeconds);
    }
}