using System;
using Godot;

namespace FirstPerson;

public partial class Player : ShootableCharacterBody3D
{
    [Export] public float cameraSensitivity = 0.01f;
    [Export] public float speed = 8;
    [Export] public float jumpVelocity = 5f;
    [Export] public int shootRange = 150;
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
    
    private double _timeInCoyoteTime;
    private bool _canJump;

    private bool _fireCameraRaycast;
    
    private float _defaultCameraHeight;
    private float _defaultColliderShapeHeight;
    
    private Camera3D _camera;
    private Node3D _hand;
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
     */

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _collisionShape3d = GetNode<CollisionShape3D>("CollisionShape3D");
        _clamberController = GetNode<ClamberController>("CollisionShape3D/ClamberController");
        _camera = GetNode<Camera3D>("Camera3D");
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
            GD.Print("Fired gun");
            _fireCameraRaycast = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
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
                //GD.Print("End coyote time");
                _canJump = false;
                _currentActionState = ActionState.InAir;
            }
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
                //GD.Print("Start coyote time");
                _currentActionState = ActionState.CoyoteTime;
                _timeInCoyoteTime = 0;
            }
            else if(_currentActionState != ActionState.CoyoteTime &&
                    _currentActionState != ActionState.InAir)
            {
                _currentActionState = ActionState.InAir;
            }
            
            tempVelocity.Y = (float) (Velocity.Y - _gravity * delta);
            //Clamber
            if(Input.IsActionPressed("Jump"))
            {
                var clamberCheck = _clamberController.AttemptClamber();
                //GD.Print($"Attempting clamber with success {clamberCheck.success}");
                if (clamberCheck.success)
                {
                    _currentActionState = ActionState.Clambering;
                    _clamberDestination = clamberCheck.result.globalPositionToClamberTo ?? Vector3.Zero;
                    _clamberDestinationXZ = new Vector2(_clamberDestination.X, _clamberDestination.Z);
                    _clamberStartPoint = GlobalPosition;
                    _clamberStartPointXZ = new Vector2(GlobalPosition.X, GlobalPosition.Z);
                    _clamberXZDirection = _clamberStartPointXZ
                        .DirectionTo(new Vector2(_clamberDestination.X, _clamberDestination.Z));
                    _clamberXZDistanceSquared = _clamberStartPointXZ.DistanceSquaredTo(_clamberDestinationXZ);
                    return;
                }
                if (Input.IsActionJustPressed("Jump") && _currentActionState is ActionState.CoyoteTime)
                {
                    //GD.Print("coyote time jump");
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
        HandleSprint();
        MoveAndSlide();
    }

    public void HandleCrouch()
    {
        if (Input.IsActionJustPressed("Crouch") && IsOnFloor())
        {
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

    public void HandleFire()
    {
        //got from https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html
        if (!_fireCameraRaycast) return;
        _fireCameraRaycast = false;
        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();
        var from = _camera.ProjectRayOrigin(mousePos);
        var to = from + _camera.ProjectRayNormal(mousePos) * shootRange;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithBodies = true;
        query.SetExclude([GetRid()]);
        query.SetCollisionMask(2);
            
        var result = spaceState.IntersectRay(query);
        if (!result.TryGetValue("collider", out var variant)) return;
        var body = (GodotObject)variant;
        if (body is not ShootableCharacterBody3D shootable) return;
        var dist = Mathf.Abs(shootable.GlobalPosition.DistanceTo(GlobalPosition));
        GD.Print($"name {shootable.Name} at distance {dist}");
        shootable.Shot(new ShotParameters(1));
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

    public void Clamber()
    {
        
        //move up to clamber Y
        if (GetBottomOfCharacter() < _clamberDestination.Y + _clamberController.clamberMargin)
        {
            //GD.Print($"Clambering up to {clamberDestination} from {GetBottomOfCharacter()}");
            Velocity = Vector3.Up * clamberVelocity;
            MoveAndSlide();
            return;
        }
        
        //move forward to clamber Z
        
        if (_clamberXZDistanceSquared > _clamberStartPointXZ.DistanceSquaredTo(new Vector2(GlobalPosition.X, GlobalPosition.Z)))
        {
            //GD.Print($"Clambering forward to {clamberDestination} from {clamberStartPoint}");
            Velocity = new Vector3(_clamberXZDirection.X, 0, _clamberXZDirection.Y) * clamberVelocity;
            MoveAndSlide();
            return;
        }
        
        ApplyFloorSnap();
        
        //GD.Print("done Clambering");
        //when done, switch movement type to onfloor
        _currentActionState = ActionState.OnFloor;
    }
}