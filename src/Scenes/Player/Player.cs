using System;
using Godot;

namespace FirstPerson;

public partial class Player : ShootableCharacterBody3D
{
    [Export] public float cameraSensitivity = 0.01f;
    [Export] public float speed = 8;
    [Export] public float jumpVelocity = 5f;
    [Export] public int shootRaycastLength = 50;
    [Export] public int interactRaycastLength = 50;
    [Export] public float interactRaycastWaitInSec = 0.2f;
    [Export] public float defaultCollisionShapePositionY;
    [Export] public float crouchCameraHeightMult = 0.4f;
    [Export] public float crouchCollisionShapeHeightMult = 0.5f;
    [Export] public float crouchAnimationInSeconds = 0.25f;
    [Export] public float crouchMovementMult = 0.6f;
    [Export] public float defaultFov;
    [Export] public float sprintFovMult = 1.05f;
    [Export] public float sprintAnimationInSeconds = 0.15f;
    [Export] public float sprintMovementMult = 1.5f;
    [Export] public float coyoteTimeInSec = 0.15f;
    [Export] public float clamberVelocity = 10f;
    
    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    
    private float _clamberXZDistanceSquared;
    private Vector3 _clamberDestination;
    private Vector2 _clamberDestinationXZ;
    private Vector3 _clamberStartPoint;
    private Vector2 _clamberStartPointXZ;
    private Vector2 _clamberXZDirection;

    private double _timeSinceLastInteractCheck;
    private double _timeInCoyoteTime;
    private bool _canJump;
    private bool _fireCameraRaycast;
    private float _defaultCameraHeight;
    private float _defaultColliderShapeHeight;
    
    private Camera3D _camera;
    private Node3D _hand;
    private RayCast3D _shootRaycast, _interactRaycast;
    private CollisionShape3D _collisionShape3d;
    private BoxShape3D _collisionBoxShape;
    private AnimationPlayer _animationPlayer;
    private ClamberController _clamberController;
    private Tween _enterCrouchTween;
    private Tween _exitCrouchTween;

    private MovementState _currentMovementState = MovementState.Default;
    private ActionState _currentActionState = ActionState.OnFloor;

    public enum MovementState
    {
        Default,
        Crouching,
        Sprinting
    }

    public enum ActionState
    {
        OnFloor,
        InAir,
        Clambering,
        CoyoteTime
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
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _collisionShape3d = GetNode<CollisionShape3D>("CollisionShape3D");
        _clamberController = GetNode<ClamberController>("CollisionShape3D/ClamberController");
        _camera = GetNode<Camera3D>("Camera3D");
        _shootRaycast = _camera.GetNode<RayCast3D>("ShootRayCast");
        _shootRaycast.AddException(this);
        _shootRaycast.SetTargetPosition(Vector3.Forward * shootRaycastLength);
        _interactRaycast = _camera.GetNode<RayCast3D>("InteractRayCast");
        _interactRaycast.AddException(this);
        _interactRaycast.SetTargetPosition(Vector3.Forward * interactRaycastLength);
        _hand = _camera.GetNode<Node3D>("Hand");
        _defaultCameraHeight = _camera.Position.Y;
        _collisionBoxShape = (BoxShape3D)_collisionShape3d.Shape;
        _defaultColliderShapeHeight = _collisionBoxShape.Size.Y;
        defaultCollisionShapePositionY = _collisionShape3d.Position.Y;
        defaultFov = _camera.Fov;
    }
    
    public float GetBottomOfCharacter() => GlobalPosition.Y - _collisionBoxShape.Size.Y / 2;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("Fire"))
        {
            _animationPlayer.Play("FireGun");
            _fireCameraRaycast = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleInteractCheck(delta);
        HandleFire();
        if (_currentActionState == ActionState.Clambering)
        {
            Clamber();
            return;
        }
        if (_currentActionState == ActionState.CoyoteTime)
        {
            _timeInCoyoteTime += delta;
            if (_timeInCoyoteTime > coyoteTimeInSec)
            {
                _canJump = false;
                _currentActionState = ActionState.InAir;
            }
        }
        HandleCrouch();
        HandleSprint();
        var movementInput = GetXZDirectionalMovement();
        var tempVelocity = Vector3.Zero;
        if (IsOnFloor())
        {
            if (!_canJump) _canJump = true;
            if (_currentActionState == ActionState.InAir)
            {
                _currentActionState = ActionState.OnFloor;
            }
            if (Input.IsActionJustPressed("Jump") && _canJump)
            {
                tempVelocity.Y = jumpVelocity;
                _canJump = false;
            }
        }
        else
        {
            if (_canJump && _currentActionState != ActionState.CoyoteTime)
            {
                _currentActionState = ActionState.CoyoteTime;
                _timeInCoyoteTime = 0;
            }
            else if(_currentActionState != ActionState.CoyoteTime &&
                    _currentActionState != ActionState.InAir)
            {
                _currentActionState = ActionState.InAir;
            }
            tempVelocity.Y = (float) (Velocity.Y - _gravity * delta);
            if(Input.IsActionPressed("Jump")) //Clamber
            {
                if (TryHandleClamber()) return;
                if (Input.IsActionJustPressed("Jump") && _currentActionState is ActionState.CoyoteTime)
                {
                    tempVelocity.Y = jumpVelocity;
                    _canJump = false;
                    _currentActionState = ActionState.InAir;
                }
            }
        }
        var movementMult = _currentMovementState switch
        {
            MovementState.Crouching => crouchMovementMult,
            MovementState.Sprinting => sprintMovementMult,
            _ => 1f
        };
        //awesome reference https://git.colormatic.org/ColormaticStudios/quality-godot-first-person/src/branch/main/addons/fpc/character.gd
        var directionV2 = movementInput.Rotated(-_camera.Rotation.Y);
        tempVelocity.X = directionV2.X * speed * movementMult;
        tempVelocity.Z = directionV2.Y * speed * movementMult;
        Velocity = tempVelocity;
        MoveAndSlide();
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            var lookDir = mouseMotionEvent.Relative;
            var rotationY = _camera.Rotation.Y - lookDir.X * cameraSensitivity;
            var rotationX = Math.Clamp(_camera.Rotation.X - lookDir.Y * cameraSensitivity, 
                Mathf.DegToRad(-90), Mathf.DegToRad(90));
            _camera.SetRotation(new Vector3(rotationX, rotationY, 0));
            _collisionShape3d.SetRotation(new Vector3(0, _camera.Rotation.Y, 0));
        }
    }
    
    public void Clamber()
    {
        if (GetBottomOfCharacter() < _clamberDestination.Y + _clamberController.clamberMargin)
        { //move up to clamber Y
            Velocity = Vector3.Up * clamberVelocity;
            MoveAndSlide();
            return;
        }
        if (_clamberXZDistanceSquared > _clamberStartPointXZ.DistanceSquaredTo(new Vector2(GlobalPosition.X, GlobalPosition.Z)))
        { //move forward to clamber Z
            Velocity = new Vector3(_clamberXZDirection.X, 0, _clamberXZDirection.Y) * clamberVelocity;
            MoveAndSlide();
            return;
        }
        ApplyFloorSnap(); //when done, switch movement type to onfloor
        _currentActionState = ActionState.OnFloor;
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
        if (_currentMovementState == MovementState.Crouching)
        {
            PlayExitCrouchAnim();
            _currentMovementState = MovementState.Default;
        }
        else
        {
            PlayEnterCrouchAnim();
            _currentMovementState = MovementState.Crouching;
        }
    }

    public void HandleSprint()
    {
        if (Input.IsActionJustPressed("Sprint") && IsOnFloor())
        {
            if (_currentMovementState == MovementState.Sprinting)
            {
                PlayExitSprintAnim();
                _currentMovementState = MovementState.Default;
            }
            else
            {
                PlayEnterSprintAnim();
                _currentMovementState = MovementState.Sprinting;
            }
        }
        else if (_currentMovementState == MovementState.Sprinting && Velocity == Vector3.Zero)
        {
            PlayExitSprintAnim();
            _currentMovementState = MovementState.Default;
        }
    }

    public bool TryHandleClamber()
    {
        var clamberCheck = _clamberController.AttemptClamber();
        if (!clamberCheck.success) return false;
        _currentActionState = ActionState.Clambering;
        _clamberDestination = clamberCheck.result.globalPositionToClamberTo ?? Vector3.Zero;
        _clamberDestinationXZ = new Vector2(_clamberDestination.X, _clamberDestination.Z);
        _clamberStartPoint = GlobalPosition;
        _clamberStartPointXZ = new Vector2(GlobalPosition.X, GlobalPosition.Z);
        _clamberXZDirection = _clamberStartPointXZ
            .DirectionTo(new Vector2(_clamberDestination.X, _clamberDestination.Z));
        _clamberXZDistanceSquared = _clamberStartPointXZ.DistanceSquaredTo(_clamberDestinationXZ);
        return true;
    }

    public void HandleFire()
    {
        if (!_fireCameraRaycast) return;
        _fireCameraRaycast = false;
        if (!_shootRaycast.IsColliding()) return;
        var collided = _shootRaycast.GetCollider();
        if (collided is ShootableCharacterBody3D shootable)
        {
            shootable.Shot(new ShotParameters(1));
        }
    }

    public void HandleInteractCheck(double delta)
    {
        if (_timeSinceLastInteractCheck < interactRaycastWaitInSec)
        {
            _timeSinceLastInteractCheck += delta;
            return;
        }
        _timeSinceLastInteractCheck = 0;
        
        if (!_interactRaycast.IsColliding()) return;
        //if interactable is on screen, turn on interact prompt
    }

    public void PlayEnterCrouchAnim()
    {
        _enterCrouchTween = GetTree().CreateTween();
        _enterCrouchTween.TweenProperty(_collisionBoxShape, "size:y",
            _defaultColliderShapeHeight * crouchCollisionShapeHeightMult, crouchAnimationInSeconds);
        _enterCrouchTween.TweenProperty(_collisionShape3d, "position:y",
            defaultCollisionShapePositionY * crouchCollisionShapeHeightMult, crouchAnimationInSeconds);
    }

    public void PlayExitCrouchAnim()
    {
        _exitCrouchTween = GetTree().CreateTween();
        _exitCrouchTween.TweenProperty(_collisionBoxShape, "size:y",
            _defaultColliderShapeHeight, crouchAnimationInSeconds);
        _exitCrouchTween.TweenProperty(_collisionShape3d, "position:y",
            defaultCollisionShapePositionY, crouchAnimationInSeconds);
    }

    public void PlayEnterSprintAnim()
    {
        var tween = CreateTween();
        tween.TweenProperty(_camera, "fov", defaultFov * sprintFovMult, sprintAnimationInSeconds);
    }

    public void PlayExitSprintAnim()
    {
        var tween = CreateTween();
        tween.TweenProperty(_camera, "fov", defaultFov, sprintAnimationInSeconds);
    }
}