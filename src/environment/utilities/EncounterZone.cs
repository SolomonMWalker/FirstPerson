using Godot;

namespace FirstPerson.scenes.enemies.test;

public partial class EncounterZone : Node3D
{
    [Signal]
    public delegate void OnCombatStartEventHandler(Node3D target);

    private bool _alerted;
    private Node3D _target;

    public void AlertZone(Node3D target)
    {
        EmitSignalOnCombatStart(target);
    }

    public bool TryGetTarget(out Node3D target)
    {
        target = null;
        if (!_alerted || _target is null) return false;
        target = _target;
        return true;

    }
}