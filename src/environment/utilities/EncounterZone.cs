using Godot;
using Godot.NativeInterop;

namespace FirstPerson.scenes.enemies.test;

public partial class EncounterZone : Node3D
{
    [Export] public Node3D Target { get; set; }

    [Signal]
    public delegate void OnCombatStartEventHandler(Node3D target);

    private bool _alerted;

    public override void _Ready()
    {
        base._Ready();
        if (Target is not null) _alerted = true;
    }

    public void AlertZone(Node3D target)
    {
        EmitSignalOnCombatStart(target);
    }

    public bool TryGetTarget(out Node3D target)
    {
        target = null;
        if (!_alerted || Target is null) return false;
        target = Target;
        return true;

    }
}