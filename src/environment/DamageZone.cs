using Godot;
using System;
using System.Collections.Generic;
using FirstPerson.Scenes.Player;

public partial class DamageZone : Area3D
{
    [Export] public float DamagePerTick = 10;
    [Export] public double TicksPerSecond = 2;

    private Dictionary<Node3D, double> _agentToTimeInHazard = [];
    private double _timePerTickInSec;
    
    //Make HazardHitbox, possibly give it direct access to agent HealthComponent so it doesn't worry about 
    //agent class when dealing damage.
    //Also, put it on its own collision layer as it will be separate from other hitboxes
    
    public override void _Ready()
    {
        base._Ready();
        _timePerTickInSec = 1.0 / TicksPerSecond;
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        foreach (var agent in _agentToTimeInHazard.Keys)
        {
            _agentToTimeInHazard[agent] += delta;
            if (_agentToTimeInHazard[agent] > _timePerTickInSec)
            {
                DamageAgentAndResetTick(agent);
            }
        }
    }

    public void OnAreaEntered(Area3D area)
    {
        if (area is not Hitbox hitbox || !_agentToTimeInHazard.TryAdd(hitbox.Parent, _timePerTickInSec)) return;
    }

    public void OnAreaExited(Area3D area)
    {
        if (area is not Hitbox hitbox || !_agentToTimeInHazard.Remove(hitbox.Parent)) return;
    }

    public void DamageAgentAndResetTick(Node3D agent)
    {
        if (agent is CombatAgent combatAgent)
        {
            combatAgent.TakeDamage(DamagePerTick);
        }
        else if (agent is PlayerController playerController)
        {
            playerController.TakeDamage(DamagePerTick);
        }

        _agentToTimeInHazard[agent] = 0;
    }
}
