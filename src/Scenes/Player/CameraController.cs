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
    
    [ExportCategory("Raycast Settings")]
    [Export] public int InteractRaycastLength { get; set; } = 50;
    [Export] public float InteractRaycastWaitInSec { get; set; } = 0.2f;
    [Export] public int ShootRaycastLength { get; set; } = 50;

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
    }

    public override void _Process(double delta)
    {
        UpdateCameraRotation(MouseCaptureComponent.MouseInput);
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
}
