using Godot;

namespace FirstPerson.Helpers;

public static class TestSpawnImpactMarker
{
    public static void SpawnImpactMarker(this Node spawner, Vector3 position)
    {
        //for 2d decal, get normal of collision, slap it there, maybe edged a bit down the normal for visibility
        var marker = new MeshInstance3D();
        var box = new BoxMesh();
        box.Size = new Vector3(0.1f, 0.1f, 0.1f);
        marker.Mesh = box;

        var material = new StandardMaterial3D();
        material.AlbedoColor = Colors.Red;
        marker.SetSurfaceOverrideMaterial(0, material);

        spawner.GetTree().CurrentScene.AddChild(marker);
        marker.GlobalPosition = position;

        spawner.GetTree().CreateTimer(2.0).Timeout += marker.QueueFree;
    }}