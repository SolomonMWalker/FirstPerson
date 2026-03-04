using Godot;
using FirstPerson.CustomTypes;
using FirstPerson.Helpers;

public partial class Explosion : Node3D
{
    private static string PathToExplosionScene = $"/explosion.tscn";
    private static PackedScene ExplosionPackedScene;
    
    public double LifetimeInSec { get; private set; } = 0.25;
    public int Damage { get; private set; }

    //private List<CollisionObject3D> ObjectsHit { get; set; } = [];
    private ShapeCast3D ShapeCast3D { get; set; }
    private Vector3 InitialGlobalPosition { get; set; }
    //private Poll TimeAlivePoll { get; set; }

    public static void Initialize(Node3D parent, Vector3 globalPosition)
    {
        ExplosionPackedScene ??= GD.Load<PackedScene>(PathToExplosionScene);
        var explosion = ExplosionPackedScene.Instantiate<Explosion>();
        explosion.InitializeValues(globalPosition);
        parent.AddChild(explosion);
    }
    
    public void InitializeValues(Vector3 globalPosition) => InitialGlobalPosition = globalPosition;

    public override void _Ready()
    {
        base._Ready();
        GlobalPosition = InitialGlobalPosition;
        ShapeCast3D = GetNode<ShapeCast3D>("ShapeCast3D");
        //TimeAlivePoll = new Poll(LifetimeInSec, 0);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        // if (TimeAlivePoll.IsPollPinged(delta))
        // {
        //     QueueFree();
        //     return;
        // }

        if (!ShapeCast3D.IsColliding()) return;
        for (int index = 0; index < ShapeCast3D.GetCollisionCount(); index++)
        {
            var collided = ShapeCast3D.GetCollider(index);
            if (collided is not CollisionObject3D colObj3D) continue;
            ShapeCast3D.AddException(colObj3D);
            //ObjectsHit.Add(colObj3D);
            
        }
    }
}