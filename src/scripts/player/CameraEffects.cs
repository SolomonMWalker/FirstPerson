using Godot;
using System;
using FirstPerson.Scenes.Player;

public partial class CameraEffects : Node3D
{
    [ExportCategory("References")]
    [Export] public PlayerController PlayerController;

    [ExportCategory("Effects")] 
    [Export] public bool EnableTilt { get; set; }
    [Export] public bool EnableFallKick { get; set; }
    [Export] public bool EnableDamageKick { get; set; }
    [Export] public bool EnableWeaponKick { get; set; }
    [Export] public bool EnableScreenShake { get; set; }
    [Export] public bool EnableHeadbob { get; set; }
    [Export] public float AimMult
    {
        get => _aimMult;
        set => _aimMult = Mathf.Clamp(value, 0, 1f);
    }
    private float _aimMult = 0.1f;
    
    [ExportCategory("Kick & Recoil Settings")]
    [ExportGroup("Run Tilt")]  
    [Export] public float RunPitch { get; set; } = 0.05f; //Degrees
    [Export] public float RunRoll { get; set; } = 0.15f;
    [Export] public float MaxPitch { get; set; } = 0.3f;
    [Export] public float MaxRoll { get; set; } = 0.6f;
    
    [ExportGroup("Fall Kick")] 
    [Export] public float FallTimeInSec { get; set; } = 0.3f;

    [ExportGroup("Damage Kick")]
    [Export] public float DamageTime { get; set; } = 0.3f;

    [ExportGroup("Weapon Kick")]
    [Export] public float WeaponDecay { get; set; } = 0.5f;

    [ExportGroup("Screen Shake")]
    [Export] public float MinimumScreenShake { get; set; } = 0.05f;
    [Export] public float MaximumScreenShake { get; set; } = 0.5f;

    [ExportGroup("Headbob")] 
    [Export] public float BobPitch
    {
        get => _bobPitch;
        set => _bobPitch = Mathf.Clamp(value, 0.0f, 0.1f);
    }
    private float _bobPitch = 0.05f;
    [Export] public float BobRoll
    {
        get => _bobRoll;
        set => _bobRoll = Mathf.Clamp(value, 0.0f, 0.1f);
    }
    private float _bobRoll = 0.025f;
    [Export] public float BobUp
    {
        get => _bobUp;
        set => _bobUp = Mathf.Clamp(value, 0.0f, 0.04f);
    }
    private float _bobUp = 0.005f;
    [Export] public float BobFrequency
    {
        get => _bobFrequency;
        set => _bobFrequency = Mathf.Clamp(value, 3.0f, 8.0f);
    }
    private float _bobFrequency = 6.0f;

    public bool Aiming { get; set; }
    private Random Random { get; set; } = new ();
    private float _tiltForwardDot, _tiltRightDot;
    private float _fallValue, _fallTimer;
    private float _damagePitch, _damageRoll, _damageTimer;
    private Vector3 _weaponKickAngles = Vector3.Zero;
    private Tween _screenShakeTween;
    private float _screenShakeAmount;
    private float _stepTimer;

    public override void _Process(double delta)
    {
        base._Process(delta);
        CalculateViewOffset(delta);

        if (Input.IsActionJustPressed("Test"))
        {
            AddScreenShake(1.0f, 5.0f);
        }
    }
    
    public void CalculateViewOffset(double delta)
    {
        if (PlayerController is null) return;
        var fDelta = (float)delta;
        var velocity = PlayerController.Velocity;
        
        //Headbob step timer and Sine value
        var speed = new Vector2(velocity.X, velocity.Z).Length();
        if (speed > 0.1 && PlayerController.IsOnFloor())
        {
            _stepTimer += (float) delta * (speed / BobFrequency);
            _stepTimer %= 1.0f;
        }
        else
        {
            _stepTimer = 0.0f;
        }

        var bobSin = Mathf.Sin(_stepTimer * 2.0 * Mathf.Pi) * 0.5; //0.5 reduces magnitude of bob
        
        var angles = Vector3.Zero;
        var offset = Vector3.Zero;

        _fallTimer -= fDelta;
        _damageTimer -= fDelta;
        
        //Run Tilt
        if (EnableTilt)
        {
            var forward = PlayerController.CameraController.GlobalTransform.Basis.Z;
            var right = PlayerController.CameraController.GlobalTransform.Basis.X;

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

        if (EnableHeadbob)
        {
            var pitchDelta = bobSin * Mathf.DegToRad(_bobPitch) * speed;
            if (Aiming) pitchDelta *= _aimMult;
            angles.X -= (float) pitchDelta;

            var rollDelta = bobSin * Mathf.DegToRad(_bobRoll) * speed;
            if (Aiming) rollDelta *= _aimMult;
            angles.Z -= (float) rollDelta;

            var bobHeight = bobSin * speed * _bobUp;
            if (Aiming) bobHeight *= _aimMult;
            offset.Y += (float) bobHeight;
        }

        PlayerController.CameraController.Camera.Position = offset;
        PlayerController.CameraController.Camera.Rotation = angles;
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

    public void AddScreenShake(float amount, float seconds)
    {
        if (!EnableScreenShake) return;
        _screenShakeTween?.Kill();
        _screenShakeAmount = amount;
        _screenShakeTween = CreateTween();
        var callable = Callable.From((float boundAlpha) => UpdateScreenShake(boundAlpha));
        _screenShakeTween.TweenMethod(callable, 0.0f, 1.0f, seconds).SetEase(Tween.EaseType.Out);
    }

    private void UpdateScreenShake(float alpha)
    {
        _screenShakeAmount = Mathf.Remap(_screenShakeAmount, 0.0f, 1.0f, MinimumScreenShake, MaximumScreenShake);
        var currentShakeAmount = _screenShakeAmount * (1.0 - alpha);
        var hRand = (float) Random.NextDouble() > 0.5 ? Random.NextDouble() : -Random.NextDouble();
        var vRand = (float) Random.NextDouble() > 0.5 ? Random.NextDouble() : -Random.NextDouble();
        var hOffset = hRand * currentShakeAmount;
        var vOffset = vRand * currentShakeAmount;
        PlayerController.CameraController.Camera.HOffset = (float) hOffset;
        PlayerController.CameraController.Camera.VOffset = (float) vOffset;
        
    }
}
