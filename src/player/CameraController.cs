using Godot;
using System;
using FirstPerson.Helpers;
using FirstPerson.Scenes.Player;

public partial class CameraController : Node3D
{
    [ExportCategory("References")]
    [Export] public Camera3D Camera { get; set; }
    [Export] public RayCast3D InteractRaycast { get; set; }
    [Export] public RayCast3D ShootRaycast { get; set; }
    [Export] public PlayerController PlayerController { get; set; }
    [Export] public MouseCaptureComponent MouseCaptureComponent { get; set; }
    [Export] public Node3D StandingLocation { get; set; }
    [Export] public Node3D CrouchingLocation { get; set; }

    [ExportCategory("Camera Settings")]
    [Export] public int DefaultFov { get; set; } = 77;

    [Export] public int AimFov { get; set; } = 38;
    
    [ExportGroup("Camera Tilt")]
    [Export] public float TiltLowerLimit
    {
        get => _tiltLowerLimit;
        set => _tiltLowerLimit = Mathf.Clamp(value, -90, -60);
    }
    private float _tiltLowerLimit = -90;
    [Export] public float TiltUpperLimit
    {
        get => _tiltUpperLimit;
        set => _tiltUpperLimit = Mathf.Clamp(value, 60, 90);
    }
    private float _tiltUpperLimit = 90;
    [ExportGroup("Crouch Movement")]
    [Export] public float CrouchAnimationLengthInSec { get; set; } = 0.175f;
    [Export] public float CrouchOffset { get; set; } = 0;
    [Export] public float CrouchSpeed { get; set; } = 3.0f;
    
    [ExportGroup("Sprint Movement")]
    [Export] public float SprintAnimationLengthInSec { get; set; } = 0.175f;
    [Export] public float SprintFovShiftMult { get; set; } = 1.05f;
    
    [ExportGroup("Aim Animation")]
    [Export] public float AimAnimationLengthInSec { get; set; } = 0.175f;

    [ExportGroup("Step Smoothing")]
    [Export] public float StepSpeed { get; set; } = 8;
    private float _targetHeight;
    private bool _stepSmoothing;
    //no offsets used, I tween the position itself to move from crouch to standing and vice versa,
        //so just hit target height


    [ExportCategory("Raycast Settings")]
    
    private int _interactRaycastLength  = 5;
    [Export] public int InteractRaycastLength
    {
        get => _interactRaycastLength;
        set
        {
            _interactRaycastLength = value;
            if(InteractRaycast is not null)
                InteractRaycast.TargetPosition = Vector3.Forward * _interactRaycastLength;
        }
    }
    
    private int _shootRaycastLength = 50;
    [Export] public int ShootRaycastLength {
        get => _shootRaycastLength;
        set
        {
            _shootRaycastLength = value;
            if(ShootRaycast is not null)
                ShootRaycast.TargetPosition = Vector3.Forward * _shootRaycastLength;
        }
    }

    public Vector3 InitialPosition { get; private set; }
    public Vector3 CameraOffset { get; private set; } = Vector3.Zero;

    private Tween EnterCrouchTween { get; set; }
    private Tween ExitCrouchTween { get; set; }
    private Tween EnterSprintTween { get; set; }
    private Tween ExitSprintTween { get; set; }
    private Tween EnterAimTween { get; set; }
    private Tween ExitAimTween { get; set; }
    private Vector3 _rotation = Vector3.Zero;
    
    public override void _Ready()
    {
        base._Ready();
        InitialPosition = Position;
        ShootRaycast.AddException(PlayerController);
        ShootRaycast.Enabled = false;
        ShootRaycastLength = ShootRaycastLength;
        InteractRaycast.SetTargetPosition(Vector3.Forward * InteractRaycastLength);
        InteractRaycast.AddException(PlayerController);
        _rotation = PlayerController.Rotation;
    }

    public override void _Process(double delta)
    {
        if (_stepSmoothing)
        {
            _targetHeight *= Mathf.Exp(-StepSpeed * (float)delta);
            if (Mathf.Abs(_targetHeight) < 0.01f)
            {
                _targetHeight = 0f;
                _stepSmoothing = false;
            }
        }

        Position = InitialPosition + CameraOffset + new Vector3(0, _targetHeight, 0);
        UpdateCameraRotation(MouseCaptureComponent.RelativeMouseInputWithSens);
    }

    public void UpdateCameraRotation(Vector2 input)
    {
        _rotation = _rotation with
        {
            X = Mathf.Clamp(_rotation.X + input.Y,
                Mathf.DegToRad(TiltLowerLimit), Mathf.DegToRad(TiltUpperLimit)),
            Y = _rotation.Y + input.X
        };

        var playerRotation = new Vector3(0f, _rotation.Y, 0f);
        var cameraRotation = new Vector3(_rotation.X, 0f, 0f);
        
        Transform = Transform with {Basis = Basis.FromEuler(cameraRotation)};
        _rotation = _rotation with { Z = 0 };
        
        PlayerController.UpdateRotation(playerRotation);
    }

    public void SmoothStep(float heightChange)
    {
        _targetHeight -= heightChange;
        _stepSmoothing = true;
    }
    
    public (GodotObject gdObj, Vector3 point) GetWhatAndWhereShootRaycastIsHitting(Vector2 rotationInDeg, int length)
    {
        ShootRaycast.Rotation = ShootRaycast.Rotation with
        {
            X = Mathf.DegToRad(rotationInDeg.X),
            Y = Mathf.DegToRad(rotationInDeg.Y)
        };
        ShootRaycastLength = length;
        ShootRaycast.ForceRaycastUpdate();
        return !ShootRaycast.IsColliding() ? (null, Vector3.Zero) : 
            (ShootRaycast.GetCollider(), ShootRaycast.GetCollisionPoint());
    }
    
    public (GodotObject gdObj, Vector3 point) GetWhatAndWhereShootRaycastIsHitting(Vector3 targetGlobalPosition)
    {
        ShootRaycast.TargetPosition = ShootRaycast.ToLocal(targetGlobalPosition);
        ShootRaycast.ForceRaycastUpdate();
        return !ShootRaycast.IsColliding() ? (null, Vector3.Zero) : 
            (ShootRaycast.GetCollider(), ShootRaycast.GetCollisionPoint());
    }
    
    public (bool successful, InteractHitbox interactable) GetWhatInteractRaycastIsHitting()
    {
        return !InteractRaycast.IsColliding() ? (false, null) : (true, (InteractHitbox)InteractRaycast.GetCollider());
    }

    public bool AreCrouchTweensRunning() => IsEnterCrouchTweenRunning() || IsExitCrouchTweenRunning();

    public bool IsEnterCrouchTweenRunning()
    {
        return EnterCrouchTween is not null && EnterCrouchTween.IsRunning();
    }
    public bool IsExitCrouchTweenRunning()
    {
        return ExitCrouchTween is not null && ExitCrouchTween.IsRunning();
    }

    public void EnterCrouchTweenActivate()
    {
        EnterCrouchTween = GetTree().CreateTween();
        EnterCrouchTween.TweenProperty(this, "CameraOffset:x", CrouchingLocation.Position.X, CrouchAnimationLengthInSec);
        EnterCrouchTween.Parallel().TweenProperty(this, "CameraOffset:y", CrouchingLocation.Position.Y, CrouchAnimationLengthInSec);
        EnterCrouchTween.Parallel().TweenProperty(this, "CameraOffset:z", CrouchingLocation.Position.Z, CrouchAnimationLengthInSec);
        EnterCrouchTween.Play();
    }

    public void ExitCrouchTweenActivate()
    {
        ExitCrouchTween = GetTree().CreateTween();
        ExitCrouchTween.TweenProperty(this, "CameraOffset:x", StandingLocation.Position.X, CrouchAnimationLengthInSec);
        ExitCrouchTween.Parallel().TweenProperty(this, "CameraOffset:y", StandingLocation.Position.Y, CrouchAnimationLengthInSec);
        ExitCrouchTween.Parallel().TweenProperty(this, "CameraOffset:z", StandingLocation.Position.Z, CrouchAnimationLengthInSec);
        ExitCrouchTween.Play();
    }
    
    public void EnterSprintTweenActivate()
    {
        EnterSprintTween = CreateTween();
        EnterSprintTween.TweenProperty(Camera, "fov", DefaultFov * SprintFovShiftMult, SprintAnimationLengthInSec);
    }

    public void ExitSprintTweenActivate()
    {
        ExitSprintTween = CreateTween();
        ExitSprintTween.TweenProperty(Camera, "fov", DefaultFov, SprintAnimationLengthInSec);
    }

    public void EnterAimTweenActivate()
    {
        EnterAimTween = CreateTween();
        EnterAimTween.TweenProperty(Camera, "fov", AimFov, AimAnimationLengthInSec);
    }
    
    public void ExitAimTweenActivate()
    {
        ExitAimTween = CreateTween();
        ExitAimTween.TweenProperty(Camera, "fov", DefaultFov, AimAnimationLengthInSec);
    }
}
