using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class IdleState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.SetCurrentNavComponent(Grunt.AgentIdleComponent);
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Grunt is null || Grunt.firing || Grunt.dead) return;
        if (Grunt.EnemyStateMachine.CurrentCombatState == "InCombatState")
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FollowState"));
        }
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt is null) return;
        
        if (Grunt.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("DeadState"));
            return;
        }
    }
}
