using Godot;

namespace FirstPerson.environment.utilities;

public partial class EncounterZone : Node3D
{
    [Export] public Node3D Target { get; set; }
    [Export] public Godot.Collections.Array<Node3D> EncounterAgents { get; set; }

    [Export] public bool encounterZoneActive = true;
    private int _agentsDead, _numAgents;

    [Signal] public delegate void OnCombatStartEventHandler(Node3D target);
    [Signal] public delegate void OnEncounterZoneDoneEventHandler();

    public bool Alerted { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        if (Target is not null) Alerted = true;
        _numAgents = EncounterAgents.Count;
    }

    public void AlertZone(Node3D target)
    {
        if (target is null || !encounterZoneActive) return;
        Alerted = true;
        EmitSignalOnCombatStart(target);
    }

    public bool TryGetTarget(out Node3D target)
    {
        target = null;
        if (!Alerted || Target is null) return false;
        target = Target;
        return true;
    }

    public void AddAgentToEncounterZone(Node3D agent)
    {
        if (!encounterZoneActive) return;
        EncounterAgents.Add(agent);
        _numAgents++;
    }

    public void AgentDied()
    {
        if (!encounterZoneActive) return;
        _agentsDead++;
        if (_agentsDead >= _numAgents)
        {
            EmitSignalOnEncounterZoneDone();
            GD.Print("encounter zone done");
            encounterZoneActive = false;
        }
    }
}