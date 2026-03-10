using Godot;
using FirstPerson.agents.spawners;

public partial class TestInteractable : MeshInstance3D
{
    [Export] public Spawner Spawner { get; set; }
    [Export] public InteractHitbox InteractHitbox { get; set; }

    public override void _Ready()
    {
        base._Ready();
        InteractHitbox.OnInteract += () =>
        {
            var surfaceMat = (StandardMaterial3D) GetSurfaceOverrideMaterial(0);
            surfaceMat.AlbedoColor = Colors.Red;
            SetSurfaceOverrideMaterial(0, surfaceMat);
            GetTree().CreateTimer(2).Timeout += () =>
            {
                var surfaceMat1 = (StandardMaterial3D) GetSurfaceOverrideMaterial(0);
                surfaceMat1.AlbedoColor = Colors.White;
                SetSurfaceOverrideMaterial(0, surfaceMat1);
            };
            Spawner.Spawn();
        };
    }
}
