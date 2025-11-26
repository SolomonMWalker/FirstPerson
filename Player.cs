using System;
using System.Linq;
using Godot;

namespace FirstPerson;

public partial class Player : ShootableCharacterBody3D
{
    public Camera3D camera;
    public RayCast3D sightRaycast;
    public CollisionShape3D collisionShape3d;
    public BoxShape3D collisionBoxShape;
    public AnimationPlayer animationPlayer;
    public ClamberController clamberController;
    public float cameraSensitivity = 0.01f;
    public float speed = 10;
    public float jumpVelocity = 5.5f;
    public float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    public float defaultCollisionShapePositionY;
    public float crouchCameraHeightMult = 0.4f;
    public float crouchCollisionShapeHeightMult = 0.5f;
    public float crouchAnimationInSeconds = 0.25f;
    public float crouchMovementMult = 0.6f;
    public float defaultFov;
    public float sprintFovMult = 1.05f;
    public float sprintAnimationInSeconds = 0.15f;
    public float sprintMovementMult = 1.5f;
    public Vector3 clamberDestination;
    public Vector2 clamberDestinationXZ;
    public Vector3 clamberStartPoint;
    public Vector2 clamberStartPointXZ;
    public Vector2 clamberXZDirection;
    public float clamberXZDistanceSquared;
    public float clamberVelocity = 10f;
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
        Clambering,
        CoyoteTime //InAir, but can still jump
    }
    
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
     */

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        sightRaycast = GetNode<RayCast3D>("Camera3D/RayCast3D");
        collisionShape3d = GetNode<CollisionShape3D>("CollisionShape3D");
        clamberController = GetNode<ClamberController>("CollisionShape3D/ClamberController");
        camera = GetNode<Camera3D>("Camera3D");
        defaultCameraHeight = camera.Position.Y;
        collisionBoxShape = (BoxShape3D)collisionShape3d.Shape;
        defaultColliderShapeHeight = collisionBoxShape.Size.Y;
        defaultCollisionShapePositionY = collisionShape3d.Position.Y;
        sightRaycast.TargetPosition = new Vector3(0, 0, -shootRange);
        defaultFov = camera.Fov;
    }
    
    public float GetBottomOfCharacter() => GlobalPosition.Y - collisionBoxShape.Size.Y / 2;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("Fire"))
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

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (currentMovementState == MovementState.Clambering)
        {
            Clamber();
            return;
        }
        HandleCrouch();
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
        
        var tempVelocity = Vector3.Zero;

        if (IsOnFloor())
        {
            
            if (Input.IsActionJustPressed("Jump"))
            {
                tempVelocity.Y = jumpVelocity;
            }
        }
        else
        {
            tempVelocity.Y = (float) (Velocity.Y - gravity * delta);
            //Clamber
            if(Input.IsActionPressed("Jump"))
            {
                var clamberCheck = clamberController.AttemptClamber();
                //GD.Print($"Attempting clamber with success {clamberCheck.success}");
                if (clamberCheck.success)
                {
                    currentMovementState = MovementState.Clambering;
                    clamberDestination = clamberCheck.result.globalPositionToClamberTo ?? Vector3.Zero;
                    clamberDestinationXZ = new Vector2(clamberDestination.X, clamberDestination.Z);
                    clamberStartPoint = GlobalPosition;
                    clamberStartPointXZ = new Vector2(GlobalPosition.X, GlobalPosition.Z);
                    clamberXZDirection = clamberStartPointXZ
                        .DirectionTo(new Vector2(clamberDestination.X, clamberDestination.Z));
                    clamberXZDistanceSquared = clamberStartPointXZ.DistanceSquaredTo(clamberDestinationXZ);
                    return;
                }
            }
        }

        var movementMult = currentMovementState switch
        {
            MovementState.Crouching => crouchMovementMult,
            MovementState.Sprinting => sprintMovementMult,
            _ => 1f
        };

        //awesome reference https://git.colormatic.org/ColormaticStudios/quality-godot-first-person/src/branch/main/addons/fpc/character.gd
        var directionV2 = movementInput.Rotated(-camera.Rotation.Y);
        tempVelocity.X = directionV2.X * speed * movementMult;
        tempVelocity.Z = directionV2.Y * speed * movementMult;
        Velocity = tempVelocity;
        HandleSprint();
        MoveAndSlide();
    }

    public void HandleCrouch()
    {
        if (Input.IsActionJustPressed("Crouch") && IsOnFloor())
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

    public void HandleSprint()
    {
        if (Input.IsActionJustPressed("Sprint") && IsOnFloor())
        {
            if (currentMovementState == MovementState.Sprinting)
            {
                PlayExitSprintAnim();
                currentMovementState = MovementState.Walking;
            }
            else
            {
                PlayEnterSprintAnim();
                currentMovementState = MovementState.Sprinting;
            }
        }
        else if (currentMovementState == MovementState.Sprinting && Velocity == Vector3.Zero)
        {
            PlayExitSprintAnim();
            currentMovementState = MovementState.Walking;
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
            collisionShape3d.SetRotation(new Vector3(0, camera.Rotation.Y, 0));
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

    public void PlayEnterSprintAnim()
    {
        var tween = CreateTween();
        tween.TweenProperty(camera, "fov", defaultFov * sprintFovMult, sprintAnimationInSeconds);
    }

    public void PlayExitSprintAnim()
    {
        var tween = CreateTween();
        tween.TweenProperty(camera, "fov", defaultFov, sprintAnimationInSeconds);
    }

    public void Clamber()
    {
        
        //move up to clamber Y
        if (GetBottomOfCharacter() < clamberDestination.Y + clamberController.clamberMargin)
        {
            //GD.Print($"Clambering up to {clamberDestination} from {GetBottomOfCharacter()}");
            Velocity = Vector3.Up * clamberVelocity;
            MoveAndSlide();
            return;
        }
        
        //move forward to clamber Z
        
        if (clamberXZDistanceSquared > clamberStartPointXZ.DistanceSquaredTo(new Vector2(GlobalPosition.X, GlobalPosition.Z)))
        {
            //GD.Print($"Clambering forward to {clamberDestination} from {clamberStartPoint}");
            Velocity = new Vector3(clamberXZDirection.X, 0, clamberXZDirection.Y) * clamberVelocity;
            MoveAndSlide();
            return;
        }
        
        //GD.Print("done Clambering");
        //when done, switch movement type to walking
        currentMovementState = MovementState.Walking;
    }
}