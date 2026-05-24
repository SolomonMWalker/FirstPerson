using Godot;
using System.Collections.Generic;
using System.Linq;
using FirstPerson.scenes.enemies.test;

[Tool]
public partial class DamageZone : Area3D
{
    [Export] public float DamagePerTick { get; set; } = 10;
    [Export] public float TicksPerSecond { get; set; } = 2;

    private readonly Dictionary<Hitbox, double> _hitboxToTimeInHazard = [];
    private double _timePerTickInSec;

    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> props)
    {
        DamagePerTick  = (float) props["damage_per_tick"];
        TicksPerSecond = (float) props["ticks_per_second"];
    }

    public void _func_godot_build_complete() { }

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        _timePerTickInSec = 1.0 / TicksPerSecond;
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        foreach (var agent in _hitboxToTimeInHazard.Keys.ToList())
        {
            _hitboxToTimeInHazard[agent] += delta;
            if (_hitboxToTimeInHazard[agent] > _timePerTickInSec)
            {
                DamageAgentAndResetTick(agent);
            }
        }
    }

    public void OnAreaEntered(Area3D area)
    {
        if (area is not Hitbox hitbox) return;
        _hitboxToTimeInHazard.TryAdd(hitbox, _timePerTickInSec);
    }

    public void OnAreaExited(Area3D area)
    {
        if (area is not Hitbox hitbox || !_hitboxToTimeInHazard.Remove(hitbox)) return;
    }

    public void DamageAgentAndResetTick(Hitbox hitbox)
    {
        hitbox.Hit(BuildHitInformation());
        _hitboxToTimeInHazard[hitbox] = 0;
    }

    public HitInformation BuildHitInformation()
    {
        return new HitInformation(
            hitType: HitType.Hazard,
            pitch: 1,
            roll: 1,
            healthDamage: (int) DamagePerTick,
            sourceGlobalPosition: GlobalPosition,
            staggerDamage: 0
        );
    }
}
