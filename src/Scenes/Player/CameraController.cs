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
    [Export] public float SprintFovMult { get; set; } = 1.05f;
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

    [ExportGroup("Step Smoothing")]
    [Export] public float StepSpeed { get; set; } = 8;
    public float OffsetHeight { get; set; }
    private const float DefaultHeight = 0.5f;
    private float _targetHeight;
    private bool _stepSmoothing;
    
    
    
    [ExportCategory("Raycast Settings")]
    [Export] public int InteractRaycastLength { get; set; } = 50;
    [Export] public float InteractRaycastWaitInSec { get; set; } = 0.2f;
    [Export] public int ShootRaycastLength { get; set; } = 50;
    
    private Tween EnterCrouchTween { get; set; }
    private Tween ExitCrouchTween { get; set; }
    private Vector3 _rotation = Vector3.Zero;
    
    public override void _Ready()
    {
        base._Ready();
        ShootRaycast.SetTargetPosition(Vector3.Forward * ShootRaycastLength);
        ShootRaycast.AddException(Player);
        ShootRaycast.Enabled = false;
        InteractRaycast.SetTargetPosition(Vector3.Forward * InteractRaycastLength);
        InteractRaycast.AddException(Player);
        InteractRaycast.Enabled = false;
        _rotation = Player.Rotation;
        OffsetHeight = DefaultHeight;
    }

    public override void _Process(double delta)
    {
        UpdateCameraRotation(MouseCaptureComponent.MouseInput);

        if (_stepSmoothing)
        {
            _targetHeight = Mathf.Lerp(_targetHeight, 0.0f, StepSpeed * (float)delta);
            if (Mathf.Abs(_targetHeight) < 0.01)
            {
                _targetHeight = 0;
                _stepSmoothing = false;
            }

            Position = Position with { Y = OffsetHeight + _targetHeight };
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

    public void UpdateCameraHeight(double delta, int direction)
    {
        if (Position.Y >= CrouchOffset && Position.Y <= DefaultHeight)
        {
            var y = (float) Mathf.Clamp(Position.Y + (CrouchSpeed * direction) * delta, CrouchOffset, DefaultHeight);
            Position = Position with { Y = y };
        }
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
        EnterCrouchTween.TweenProperty(this, "position:x", CrouchingLocation.Position.X, CrouchAnimationLengthInSec);
        EnterCrouchTween.Parallel().TweenProperty(this, "position:y", CrouchingLocation.Position.Y, CrouchAnimationLengthInSec);
        EnterCrouchTween.Parallel().TweenProperty(this, "position:z", CrouchingLocation.Position.Z, CrouchAnimationLengthInSec);
        EnterCrouchTween.Play();
    }

    public void ExitCrouchTweenActivate()
    {
        ExitCrouchTween = GetTree().CreateTween();
        ExitCrouchTween.TweenProperty(this, "position:x", StandingLocation.Position.X, CrouchAnimationLengthInSec);
        ExitCrouchTween.Parallel().TweenProperty(this, "position:y", StandingLocation.Position.Y, CrouchAnimationLengthInSec);
        ExitCrouchTween.Parallel().TweenProperty(this, "position:z", StandingLocation.Position.Z, CrouchAnimationLengthInSec);
        ExitCrouchTween.Play();
    }
}
