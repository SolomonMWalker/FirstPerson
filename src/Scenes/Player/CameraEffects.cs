using Godot;
using System;
using FirstPerson.Scenes.Player;

public partial class CameraEffects : Node3D
{
    [ExportCategory("References")]
    [Export] public Player Player;
    [ExportCategory("Effects")] 
    [Export] public bool EnableTilt { get; set; }
    [Export] public bool EnableFallKick { get; set; }
    [ExportCategory("Kick & Recoil Settings")]
    [ExportGroup("Run Tilt")]  
    [Export] public float RunPitch { get; set; } = 0.05f; //Degrees
    [Export] public float RunRoll { get; set; } = 0.15f;
    [Export] public float MaxPitch { get; set; } = 0.3f;
    [Export] public float MaxRoll { get; set; } = 0.6f;
    [ExportGroup("Fall Kick")] 
    [Export] public float FallTimeInSec { get; set; } = 0.3f;
    
    private float _forwardDot, _rightDot;
    private float _fallValue, _fallTimer;

    public override void _Process(double delta)
    {
        base._Process(delta);
        CalculateViewOffset(delta);
    }
    
    //continuing at https://youtu.be/53Awc2twnhA?si=UiiAgJvv0Dt05RPd&t=559

    public void CalculateViewOffset(double delta)
    {
        if (Player is null) return;
        var velocity = Player.Velocity;
        var angles = Vector3.Zero;
        var offset = Vector3.Zero;

        _fallTimer -= (float) delta;
        
        //Run Tilt
        if (EnableTilt)
        {
            var forward = Player.CameraController.GlobalTransform.Basis.Z;
            var right = Player.CameraController.GlobalTransform.Basis.X;

            _forwardDot = Mathf.Lerp(_forwardDot, velocity.Dot(forward), 0.5f);
            var forwardTilt = Mathf.Clamp(_forwardDot * Mathf.DegToRad(RunPitch), Mathf.DegToRad(-MaxPitch),
                Mathf.DegToRad(MaxPitch));
            angles.X += forwardTilt;

            _rightDot = Mathf.Lerp(_rightDot, velocity.Dot(right), 0.5f);
            var sideTilt = Mathf.Clamp(_rightDot * Mathf.DegToRad(RunRoll), Mathf.DegToRad(-MaxRoll),
                Mathf.DegToRad(MaxRoll));
            angles.Z -= sideTilt;
        }
        
        //Fall Kick
        if (EnableFallKick)
        {
            var fallRatio = Mathf.Max(0.0, _fallTimer / FallTimeInSec);
            var fallKickAmount = fallRatio * _fallValue;
            angles.X -= (float) fallKickAmount;
            offset.Y -= (float)fallKickAmount;
        }

        Player.CameraController.Camera.Position = offset;
        Player.CameraController.Camera.Rotation = angles;
    }

    public void AddFallKick(float fallStrength)
    {
        _fallValue = Mathf.DegToRad(fallStrength);
        _fallTimer = FallTimeInSec;
    }
}
