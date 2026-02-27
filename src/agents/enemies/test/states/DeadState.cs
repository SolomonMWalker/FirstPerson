using Godot;
using FirstPerson.scenes.enemies.test.states;

public partial class DeadState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.NavigationAgent3D.TargetPosition = Grunt.NavigationAgent3D.TargetPosition;
        Grunt.OnVelocityComputed(Vector3.Zero);
    }
}
