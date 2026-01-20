using Godot;
using System;
using FirstPerson.Scenes.Player;

public partial class CameraEffects : Node3D
{
    [ExportCategory("References")]
    [Export] public Player Player;

    [Export] public Node3D DamageSource;

    [ExportCategory("Effects")] 
    [Export] public bool EnableTilt { get; set; }
    [Export] public bool EnableFallKick { get; set; }
    [Export] public bool EnableDamageKick { get; set; }
    [Export] public bool EnableWeaponKick { get; set; }
    
    [ExportCategory("Kick & Recoil Settings")]
    [ExportGroup("Run Tilt")]  
    [Export] public float RunPitch { get; set; } = 0.05f; //Degrees
    [Export] public float RunRoll { get; set; } = 0.15f;
    [Export] public float MaxPitch { get; set; } = 0.3f;
    [Export] public float MaxRoll { get; set; } = 0.6f;
    
    [ExportGroup("Fall Kick")] 
    [Export] public float FallTimeInSec { get; set; } = 0.3f;

    [ExportSubgroup("Damage Kick")]
    [Export] public float DamageTime { get; set; } = 0.3f;

    [ExportSubgroup("Weapon Kick")]
    [Export] public float WeaponDecay { get; set; } = 0.5f;

    private Random Random { get; set; } = new ();
    private float _tiltForwardDot, _tiltRightDot;
    private float _fallValue, _fallTimer;
    private float _damagePitch, _damageRoll, _damageTimer;
    public Vector3 _weaponKickAngles = Vector3.Zero;

    public override void _Process(double delta)
    {
        base._Process(delta);
        CalculateViewOffset(delta);

        if (Input.IsActionJustPressed("Test"))
        {
            AddWeaponKick(5f, 5f, 5f);
        }
    }
    
    //continuing at https://youtu.be/53Awc2twnhA?si=utUGccTiSYzWb485&t=745

    public void CalculateViewOffset(double delta)
    {
        if (Player is null) return;
        var fDelta = (float)delta;
        var velocity = Player.Velocity;
        var angles = Vector3.Zero;
        var offset = Vector3.Zero;

        _fallTimer -= fDelta;
        _damageTimer -= fDelta;
        
        //Run Tilt
        if (EnableTilt)
        {
            var forward = Player.CameraController.GlobalTransform.Basis.Z;
            var right = Player.CameraController.GlobalTransform.Basis.X;

            _tiltForwardDot = Mathf.Lerp(_tiltForwardDot, velocity.Dot(forward), 0.5f);
            var forwardTilt = Mathf.Clamp(_tiltForwardDot * Mathf.DegToRad(RunPitch), Mathf.DegToRad(-MaxPitch),
                Mathf.DegToRad(MaxPitch));
            angles.X += forwardTilt;

            _tiltRightDot = Mathf.Lerp(_tiltRightDot, velocity.Dot(right), 0.5f);
            var sideTilt = Mathf.Clamp(_tiltRightDot * Mathf.DegToRad(RunRoll), Mathf.DegToRad(-MaxRoll),
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

        if (EnableDamageKick)
        {
            var damageRatio = (float) Mathf.Max(0.0, _damageTimer / DamageTime);
            //damageRatio = Mathf.Ease(damageRatio, -2); //if you want to ease over time
            angles.X += damageRatio * _damagePitch;
            angles.Z += damageRatio * _damageRoll;
        }

        if (EnableWeaponKick)
        {
            _weaponKickAngles = _weaponKickAngles.MoveToward(Vector3.Zero, WeaponDecay * fDelta);
            angles += _weaponKickAngles;
        }

        Player.CameraController.Camera.Position = offset;
        Player.CameraController.Camera.Rotation = angles;
    }

    public void AddFallKick(float fallStrength)
    {
        _fallValue = Mathf.DegToRad(fallStrength);
        _fallTimer = FallTimeInSec;
    }

    public void AddDamageKick(float pitch, float roll, Vector3 source)
    {
        var forward = GlobalTransform.Basis.Z;
        var right = GlobalTransform.Basis.X;
        var direction = GlobalPosition.DirectionTo(source);
        var forwardDot = direction.Dot(forward);
        var rightDot = direction.Dot(right);
        _damagePitch = Mathf.DegToRad(pitch) * forwardDot;
        _damageRoll = Mathf.DegToRad(roll) * rightDot;
        _damageTimer = DamageTime;
    }

    public void AddWeaponKick(float pitch, float yaw, float roll)
    {
        var randYaw = (float) Random.NextDouble() > 0.5 ? Random.NextDouble() : -Random.NextDouble();
        var randRoll = (float) Random.NextDouble() > 0.5 ? Random.NextDouble() : -Random.NextDouble();
        _weaponKickAngles.X += Mathf.DegToRad(pitch);
        _weaponKickAngles.Y += Mathf.DegToRad((float) randYaw * yaw);
        _weaponKickAngles.Z += Mathf.DegToRad((float) randRoll * roll);
    }
}
