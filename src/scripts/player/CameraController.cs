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
    [Export] public Player Player { get; set; }
    [Export] public MouseCaptureComponent MouseCaptureComponent { get; set; }
    [Export] public Node3D StandingLocation { get; set; }
    [Export] public Node3D CrouchingLocation { get; set; }

    [ExportCategory("Camera Settings")]
    [Export] public int Fov { get; set; } = 90;
    
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

    [ExportGroup("Step Smoothing")]
    [Export] public float StepSpeed { get; set; } = 8;
    private float _targetHeight;
    private bool _stepSmoothing;
    //no offsets used, I tween the position itself to move from crouch to standing and vice versa,
        //so just hit target height
    
    
    [ExportCategory("Raycast Settings")]
    [Export] public int InteractRaycastLength { get; set; } = 50;
    [Export] public float InteractRaycastWaitInSec { get; set; } = 0.2f;
    [Export] public int ShootRaycastLength { get; set; } = 50;

    public Vector3 InitialPosition { get; private set; }
    public Vector3 CameraOffset { get; private set; } = Vector3.Zero;

    private Tween EnterCrouchTween { get; set; }
    private Tween ExitCrouchTween { get; set; }
    private Tween EnterSprintTween { get; set; }
    private Tween ExitSprintTween { get; set; }
    private Vector3 _rotation = Vector3.Zero;
    
    public override void _Ready()
    {
        base._Ready();
        InitialPosition = Position;
        ShootRaycast.SetTargetPosition(Vector3.Forward * ShootRaycastLength);
        ShootRaycast.AddException(Player);
        ShootRaycast.Enabled = false;
        InteractRaycast.SetTargetPosition(Vector3.Forward * InteractRaycastLength);
        InteractRaycast.AddException(Player);
        InteractRaycast.Enabled = false;
        _rotation = Player.Rotation;
    }

    public override void _Process(double delta)
    {
        Position = InitialPosition + CameraOffset;
        UpdateCameraRotation(MouseCaptureComponent.MouseInput);

        if (_stepSmoothing)
        {
            _targetHeight = Mathf.Lerp(_targetHeight, 0.0f, StepSpeed * (float)delta);
            if (Mathf.Abs(_targetHeight) < 0.01)
            {
                _targetHeight = 0;
                _stepSmoothing = false;
            }

            Position = Position with { Y = _targetHeight };
        }
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
        
        Player.UpdateRotation(playerRotation);
    }

    public void SmoothStep(float heightChange)
    {
        _targetHeight -= heightChange;
        _stepSmoothing = true;
    }
    
    public GodotObject GetWhatInteractRaycastIsHitting()
    {
        InteractRaycast.ForceRaycastUpdate();
        return !InteractRaycast.IsColliding() ? null : InteractRaycast.GetCollider();
    }
    
    public GodotObject GetWhatShootRaycastIsHitting()
    {
        ShootRaycast.ForceRaycastUpdate();
        return !ShootRaycast.IsColliding() ? null : ShootRaycast.GetCollider();
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
        EnterSprintTween.TweenProperty(Camera, "fov", Fov * SprintFovShiftMult, SprintAnimationLengthInSec);
    }

    public void ExitSprintTweenActivate()
    {
        ExitSprintTween = CreateTween();
        ExitSprintTween.TweenProperty(Camera, "fov", Fov, SprintAnimationLengthInSec);
    }
}
