using Godot;
using FirstPerson.scenes.enemies.test.states;

public partial class DeadState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        CombatAgent.NavigationAgent3D.TargetPosition = CombatAgent.NavigationAgent3D.TargetPosition;
        CombatAgent.OnVelocityComputed(Vector3.Zero);
        //CombatAgent.CustomAnimationTree.TrySetParam("dead", true);
    }
}
