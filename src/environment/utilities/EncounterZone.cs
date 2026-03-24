using Godot;
using Godot.NativeInterop;

namespace FirstPerson.scenes.enemies.test;

public partial class EncounterZone : Node3D
{
    [Export] public Node3D Target { get; set; }

    [Signal]
    public delegate void OnCombatStartEventHandler(Node3D target);

    public bool Alerted { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        if (Target is not null) Alerted = true;
    }

    public void AlertZone(Node3D target)
    {
        if (target is null) return;
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
}